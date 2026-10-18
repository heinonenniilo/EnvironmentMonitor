using Azure.Core;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Utils;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class UserAuthService : IUserAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILogger<UserAuthService> _logger;
        private readonly IEmailClient _emailClient;
        private readonly IEmailRepository _emailRepository;
        private readonly MeasurementDbContext _measurementDbContext;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ApplicationSettings _applicationSettings;
        private readonly IDateService _dateService;
        private readonly IHangfireJobService _hangfireJobService;

        public UserAuthService(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IDeviceRepository deviceRepository,
            ILogger<UserAuthService> logger,
            IEmailClient emailClient,
            IEmailRepository emailRepository,
            MeasurementDbContext measurementDbContext,
            ApplicationDbContext applicationDbContext,
            ApplicationSettings applicationSettings,
            IDateService dateService,
            IHangfireJobService hangfireJobService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _deviceRepository = deviceRepository;
            _logger = logger;
            _emailClient = emailClient;
            _emailRepository = emailRepository;
            _measurementDbContext = measurementDbContext;
            _applicationDbContext = applicationDbContext;
            _applicationSettings = applicationSettings;
            _dateService = dateService;
            _hangfireJobService = hangfireJobService;
        }

        public async Task<LoginResultModel> Login(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                throw new UnauthorizedAccessException();
            }

            // Check if email is confirmed
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new InvalidOperationException("Email not confirmed. Please check your email and confirm your account before logging in.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException();
            }

            var mfaStatus = await BuildMfaStatus(user);
            if (mfaStatus.TwoFactorEnabled)
            {
                // Let Identity create the two factor cookie which holds the pending user.
                var twoFactorResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.Persistent, lockoutOnFailure: false);
                if (!twoFactorResult.RequiresTwoFactor)
                {
                    _logger.LogError($"Two factor was expected for user {user.Id} but Identity did not require it.");
                    throw new UnauthorizedAccessException();
                }
                await SendMfaEmailCodeIfOnlyMethod(user, mfaStatus);
                return new LoginResultModel
                {
                    Success = false,
                    RequiresTwoFactor = true,
                    AvailableMethods = mfaStatus.EnabledMethods
                };
            }

            var calculatedClaims = await GetCalculatedClaims(user);
            await _signInManager.SignInWithClaimsAsync(user, model.Persistent, calculatedClaims);
            return new LoginResultModel { Success = true };
        }

        public Task Logout()
        {
            return _signInManager.SignOutAsync();
        }

        public async Task<ExternalLoginResult> LoginWithExternalProvider(ExternalLoginModel model)
        {
            string loginProvider;
            string providerKey;
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return new ExternalLoginResult
                {
                    Success = false,
                    Errors = new List<string> { "Failed to retrieve external login information" },
                    ErrorCode = "EXTERNAL_LOGIN_INFO_NULL"
                };
            }
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var upn = info.Principal.FindFirstValue(ClaimTypes.Upn);
            if (string.IsNullOrEmpty(email))
            {
                return new ExternalLoginResult
                {
                    Success = false,
                    Errors = new List<string> { "Failed to fetch email from external provider" },
                    ErrorCode = "EMAIL_NOT_PROVIDED"
                };
            }
            if (!string.IsNullOrEmpty(model.LoginProvider) && !string.IsNullOrEmpty(model.ProviderKey))
            {
                loginProvider = model.LoginProvider;
                providerKey = model.ProviderKey;
            }
            else
            {
                loginProvider = info.LoginProvider;
                providerKey = info.ProviderKey;
            }

            if (string.IsNullOrEmpty(loginProvider) || string.IsNullOrEmpty(providerKey))
            {
                return new ExternalLoginResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid login provider or provider key" },
                    ErrorCode = "INVALID_PROVIDER_INFO"
                };
            }

            var user = await _userManager.FindByLoginAsync(loginProvider, providerKey);
            if (user != null)
            {
                var mfaStatus = await BuildMfaStatus(user);
                if (mfaStatus.TwoFactorEnabled)
                {
                    // Store the upn so that it can be added to the claims once the challenge is completed.
                    await _userManager.SetAuthenticationTokenAsync(
                        user,
                        ApplicationConstants.MfaTokenProviderName,
                        ApplicationConstants.MfaPendingUpnTokenName,
                        upn ?? string.Empty);

                    // Let Identity create the two factor cookie which holds the pending user and the login provider.
                    var externalResult = await _signInManager.ExternalLoginSignInAsync(loginProvider, providerKey, model.Persistent, bypassTwoFactor: false);
                    if (!externalResult.RequiresTwoFactor)
                    {
                        _logger.LogError($"Two factor was expected for user {user.Id} but Identity did not require it.");
                        return new ExternalLoginResult
                        {
                            Success = false,
                            Errors = new List<string> { "Failed to start two factor authentication" },
                            ErrorCode = "TWO_FACTOR_START_FAILED",
                            LoginProvider = loginProvider
                        };
                    }
                    await SendMfaEmailCodeIfOnlyMethod(user, mfaStatus);
                    return new ExternalLoginResult
                    {
                        Success = false,
                        RequiresTwoFactor = true,
                        AvailableMethods = mfaStatus.EnabledMethods,
                        LoginProvider = loginProvider
                    };
                }

                var additionalClaims = await GetCalculatedClaims(user);
                additionalClaims.Add(new Claim(ApplicationConstants.ExternalLoginProviderClaim, loginProvider));
                additionalClaims.Add(new Claim(ClaimTypes.Upn, upn ?? string.Empty));
                await _signInManager.SignInWithClaimsAsync(user, model.Persistent, additionalClaims);
                return new ExternalLoginResult { Success = true, LoginProvider = loginProvider };
            }
            else
            {
                user = new ApplicationUser { UserName = model.UserName ?? email, Email = email };
                // Check if user exists. 
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    return new ExternalLoginResult
                    {
                        Success = false,
                        Errors = [$"Login with external provider ({loginProvider}) successful but user with email {user.Email} already exists."],
                        ErrorCode = "USER_ALREADY_EXISTS",
                        LoginProvider = loginProvider
                    };
                }
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    _logger.LogError($"Failed to create user {user.Email} from external login. Errors: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    return new ExternalLoginResult
                    {
                        Success = false,
                        Errors = ["Creating user failed"],
                        ErrorCode = "USER_CREATION_FAILED",
                        LoginProvider = loginProvider
                    };
                }
                // Registered role
                var addRoleResult = await _userManager.AddToRoleAsync(user, GlobalRoles.Registered.ToString());
                if (!addRoleResult.Succeeded)
                {
                    _logger.LogError($"Failed to add Registered role to user {user.Email}. Errors: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                    return new ExternalLoginResult
                    {
                        Success = false,
                        Errors = ["User was created but process was not completed successfully."],
                        ErrorCode = "ADD_ROLE_FAILED",
                        LoginProvider = loginProvider
                    };
                }

                var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(
                    loginProvider,
                    providerKey, providerKey
                    ));

                if (!addLoginResult.Succeeded)
                {
                    _logger.LogError($"Failed to add external login to user {user.Email}. Errors: {string.Join(", ", addLoginResult.Errors.Select(e => e.Description))}");
                    return new ExternalLoginResult
                    {
                        Success = false,
                        Errors = [$"Failed to link the user with external login provider. ({loginProvider})"],
                        ErrorCode = "ADD_LOGIN_FAILED",
                        LoginProvider = loginProvider
                    };
                }

                var userToLogIn = await _userManager.FindByLoginAsync(loginProvider, providerKey);
                if (userToLogIn == null)
                {
                    return new ExternalLoginResult
                    {
                        Success = false,
                        Errors = new List<string> { "Failed to retrieve user after creation" },
                        ErrorCode = "USER_RETRIEVAL_FAILED",
                        LoginProvider = loginProvider
                    };
                }

                var additionalClaims = new List<Claim>
                {
                    new Claim(ApplicationConstants.ExternalLoginProviderClaim, loginProvider),
                    new Claim (ClaimTypes.Upn, upn ?? string.Empty)
                };
                await _signInManager.SignInWithClaimsAsync(userToLogIn, model.Persistent, additionalClaims);
                return new ExternalLoginResult { Success = true, LoginProvider = loginProvider };
            }
        }

        public async Task RegisterUser(RegisterUserModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                throw new DuplicateEntityException($"User with email '{model.Email}' already exists");
            }
            var baseUrl = string.IsNullOrEmpty(model.BaseUrl) ? _applicationSettings.BaseUrl : model.BaseUrl;
            var registerUserPath = "api/authentication/confirm-email";

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException("Failed to create user");
            _logger.LogInformation($"User with email {model.Email} created.");
            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // Build confirmation URL using the full path from model
            var queryParams = new StringBuilder();
            queryParams.Append($"?userId={Uri.EscapeDataString(user.Id)}");
            queryParams.Append($"&token={Uri.EscapeDataString(token)}");
            var confirmationUrl = UriUtils.CombineUrl(baseUrl, registerUserPath) + queryParams.ToString();

            var emailTemplate = await _emailRepository.GetEmailTemplate(EmailTemplateTypes.ConfirmUserEmail);

            if (string.IsNullOrEmpty(emailTemplate?.Message) || string.IsNullOrEmpty(emailTemplate?.Title))
            {
                _logger.LogError("Email template for ConfirmUserEmail is missing or incomplete.");
                throw new InvalidOperationException("Email template for ConfirmUserEmail is missing or incomplete.");
            }
            // Send confirmation email
            await _emailClient.SendEmailAsync(new SendEmailOptions
            {
                ToAddresses = new List<string> { user.Email! },
                Subject = emailTemplate.Title,
                HtmlContent = emailTemplate.Message,
                ReplaceTokens = new Dictionary<string, string>
                {
                    { ApplicationConstants.EmailTemplateConfirmationLinkKey, confirmationUrl },
                    { ApplicationConstants.QueuedMessageApplicationBaseUrlKey, baseUrl},
                }
            });
            _logger.LogInformation($"Confirmation email sent to {model.Email}");
        }

        public async Task<bool> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found for email confirmation: {userId}");
                return false;
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Email confirmed for user: {user.Email}");
                // Add registed role
                var addToRegisteredRoleResult = await _userManager.AddToRoleAsync(user, GlobalRoles.Registered.ToString());
                if (!addToRegisteredRoleResult.Succeeded)
                {
                    _logger.LogWarning($"Failed to add Registered role to user: {user.Email}. Errors: {string.Join(", ", addToRegisteredRoleResult.Errors.Select(e => e.Description))}");
                    return false;
                }
                return true;
            }
            _logger.LogWarning($"Email confirmation failed for user: {user.Email}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            return false;
        }

        public async Task ChangePassword(string userId, ChangePasswordModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found for password change: {userId}");
                throw new InvalidOperationException("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning($"Password change failed for user: {user.Email}. Errors: {errors}");
                throw new InvalidOperationException($"Password change failed: {errors}");
            }
            
            _logger.LogInformation($"Password changed successfully for user: {user.Email}");
        }

        public async Task ForgotPassword(ForgotPasswordModel model)
        {
            var setNewPasswordPath = "set-new-password";
            var baseUrl = string.IsNullOrEmpty(model.BaseUrl) ? _applicationSettings.BaseUrl : model.BaseUrl;
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogError($"Forgot password requested for non-existent or unconfirmed email: {model.Email}");
                throw new InvalidOperationException($"Forgot password requested for non-existent or unconfirmed email: {model.Email}");
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var queryParams = new StringBuilder();
            queryParams.Append($"?token={Uri.EscapeDataString(token)}");
            var resetUrl = UriUtils.CombineUrl(baseUrl, setNewPasswordPath) + queryParams.ToString();

            var emailTemplate = await _emailRepository.GetEmailTemplate(EmailTemplateTypes.UserPasswordReset);
            if (string.IsNullOrEmpty(emailTemplate?.Message) || string.IsNullOrEmpty(emailTemplate?.Title))
            {
                throw new InvalidOperationException("Email template for UserPasswordReset is missing or incomplete.");
            }
            // Send reset email
            await _emailClient.SendEmailAsync(new SendEmailOptions
            {
                ToAddresses = new List<string> { user.Email! },
                Subject = emailTemplate.Title,
                HtmlContent = emailTemplate.Message,
                ReplaceTokens = new Dictionary<string, string>
                {
                    { ApplicationConstants.EmailTemplatePasswordResetLinkKey, resetUrl },
                    { ApplicationConstants.QueuedMessageApplicationBaseUrlKey, baseUrl},
                }
            });
            _logger.LogInformation($"Password reset email sent to {model.Email}");
        }

        public async Task<bool> ResetPassword(ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning($"User not found for password reset: {model.Email}");
                return false;
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            
            if (result.Succeeded)
            {
                _logger.LogInformation($"Password reset successfully for user: {user.Email}");
                return true;
            }
            
            _logger.LogWarning($"Password reset failed for user: {user.Email}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            return false;
        }

        public async Task DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found for deletion: {userId}");
                throw new InvalidOperationException("User not found");
            }

            // Clear UpdatedById references for all users that were updated by this user
            var usersUpdatedByThisUser = await _applicationDbContext.Users
                .Where(u => u.UpdatedById == userId)
                .ToListAsync();

            if (usersUpdatedByThisUser.Any())
            {
                _logger.LogInformation($"Clearing UpdatedById reference for {usersUpdatedByThisUser.Count} users");
                foreach (var affectedUser in usersUpdatedByThisUser)
                {
                    affectedUser.UpdatedById = null;
                }
            }

            var userClaimsUpdatedByThisUser = await _applicationDbContext.UserClaims
                .Where(c => c.UpdatedById == userId)
                .ToListAsync();

            if (userClaimsUpdatedByThisUser.Any())
            {
                _logger.LogInformation($"Clearing UpdatedById reference for {userClaimsUpdatedByThisUser.Count} user claims");
                foreach (var affectedClaim in userClaimsUpdatedByThisUser)
                {
                    affectedClaim.UpdatedById = null;
                }
            }

            _logger.LogInformation($"Calling _userManager.DeleteAsync");
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"Failed to delete user {userId}: {errors}");
                throw new InvalidOperationException($"Failed to delete user: {errors}");
            }

            _logger.LogInformation($"User deleted: {userId}");
        }

        public async Task<List<UserInfoModel>> GetAllUsers()
        {
            _logger.LogInformation("Fetching all users");
            var users = _userManager.Users.ToList();
            var userInfoList = new List<UserInfoModel>();

            // Fetch the enabled MFA methods for all users with a single query.
            var mfaMethodsByUser = (await _applicationDbContext.UserMfaMethods
                .Where(x => x.Enabled)
                .Select(x => new { x.UserId, x.Method })
                .ToListAsync())
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Method).OrderBy(x => x).ToList());

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = await _userManager.GetClaimsAsync(user);
                var logins = await _userManager.GetLoginsAsync(user);
                var mfaMethods = mfaMethodsByUser.TryGetValue(user.Id, out var methods) ? methods : new List<MfaMethods>();

                userInfoList.Add(new UserInfoModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList(),
                    Claims = claims.ToList(),
                    ExternalLogins = logins.Select(l => new Domain.Models.ExternalLoginInfoModel
                    {
                        LoginProvider = l.LoginProvider,
                        ProviderKey = l.ProviderKey,
                        ProviderDisplayName = l.ProviderDisplayName
                    }).ToList(),
                    LockoutEnd = user.LockoutEnd?.UtcDateTime,
                    LockoutEnabled = user.LockoutEnabled,
                    AccessFailedCount = user.AccessFailedCount,
                    TwoFactorEnabled = mfaMethods.Any(),
                    MfaMethods = mfaMethods,
                    Updated = user.Updated,
                    UpdatedById = user.UpdatedById
                });
            }

            _logger.LogInformation($"Fetched {userInfoList.Count} users");
            return userInfoList;
        }

        public async Task<UserInfoModel?> GetUser(string userId)
        {
            _logger.LogInformation($"Fetching user: {userId}");
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning($"User not found: {userId}");
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var logins = await _userManager.GetLoginsAsync(user);
            var mfaMethods = await GetEnabledMfaMethods(user.Id);

            return new UserInfoModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList(),
                Claims = claims.ToList(),
                ExternalLogins = logins.Select(l => new ExternalLoginInfoModel
                {
                    LoginProvider = l.LoginProvider,
                    ProviderKey = l.ProviderKey,
                    ProviderDisplayName = l.ProviderDisplayName
                }).ToList(),
                LockoutEnd = user.LockoutEnd?.UtcDateTime,
                LockoutEnabled = user.LockoutEnabled,
                AccessFailedCount = user.AccessFailedCount,
                TwoFactorEnabled = mfaMethods.Any(),
                MfaMethods = mfaMethods,
                Updated = user.Updated,
                UpdatedById = user.UpdatedById
            };
        }

        public async Task ManageUserClaims(string userId, List<Claim>? claimsToAdd, List<Claim>? claimsToRemove)
        {
            _logger.LogInformation($"Managing claims for user: {userId}. Adding: {claimsToAdd?.Count ?? 0}, Removing: {claimsToRemove?.Count ?? 0}");
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning($"User not found: {userId}");
                throw new ArgumentException("User not found");
            }

            var errors = new List<string>();

            var userCurrentClaims = await _userManager.GetClaimsAsync(user);

            // Validate before proceeding
            var duplicateClaimsToAdd = claimsToAdd?
                .Where(c => userCurrentClaims.Any(currentClaim => currentClaim.Type.Equals(c.Type) && currentClaim.Value.Equals(c.Value, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (duplicateClaimsToAdd != null && duplicateClaimsToAdd.Any())
            {
                throw new ArgumentException($"The following claims already exist for the user and cannot be added again: {string.Join(", ", duplicateClaimsToAdd.Select(c => $"{c.Type}:{c.Value}"))}");
            }

            var nonExistingClaimsToRemove = claimsToRemove?
                .Where(c => !userCurrentClaims.Any(currentClaim => currentClaim.Type.Equals(c.Type) && currentClaim.Value.Equals(c.Value, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (nonExistingClaimsToRemove != null && nonExistingClaimsToRemove.Any())
            {
                throw new ArgumentException($"The following claims do not exist for the user and cannot be removed: {string.Join(", ", nonExistingClaimsToRemove.Select(c => $"{c.Type}:{c.Value}"))}");
            }

            if (claimsToAdd?.Any() == true)
            {
                var result = await _userManager.AddClaimsAsync(user, claimsToAdd);

                if (!result.Succeeded)
                {
                    var error = $"Failed to add claims: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                    _logger.LogError(error);
                    errors.Add(error);
                }
                else
                {
                    var addedClaimsList = claimsToAdd.Select(c => $"{c.Type}:{c.Value}");
                    _logger.LogInformation($"Successfully added claims [{string.Join(", ", addedClaimsList)}] to user: {userId}");
                }
            }

            // Remove claims
            if (claimsToRemove?.Any() == true)
            {
                var result = await _userManager.RemoveClaimsAsync(user, claimsToRemove);

                if (!result.Succeeded)
                {
                    var error = $"Failed to remove claims: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                    _logger.LogError(error);
                    errors.Add(error);
                }
                else
                {
                    var removedClaimsList = claimsToRemove.Select(c => $"{c.Type}:{c.Value}");
                    _logger.LogInformation($"Successfully removed claims [{string.Join(", ", removedClaimsList)}] from user: {userId}");
                }
            }

            if (errors.Any())
            {
                throw new InvalidOperationException($"Some operations failed: {string.Join("; ", errors)}");
            }

            _logger.LogInformation($"Successfully managed claims for user: {userId}");
        }

        public async Task ManageUserRoles(string userId, List<string>? rolesToAdd, List<string>? rolesToRemove)
        {
            _logger.LogInformation($"Managing roles for user: {userId}. Adding: {rolesToAdd?.Count ?? 0}, Removing: {rolesToRemove?.Count ?? 0}");
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogWarning($"User not found: {userId}");
                throw new InvalidOperationException("User not found");
            }

            var errors = new List<string>();

            var userCurrentRoles = await _userManager.GetRolesAsync(user);

            // Validate before proceeding
            var duplicateRolesToAdd = rolesToAdd?
                .Where(r => userCurrentRoles.Any(currentRole => currentRole.Equals(r, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (duplicateRolesToAdd != null && duplicateRolesToAdd.Any())
            {
                throw new ArgumentException($"The following roles already exist for the user and cannot be added again: {string.Join(", ", duplicateRolesToAdd)}");
            }

            var nonExistingRolesToRemove = rolesToRemove?
                .Where(r => !userCurrentRoles.Any(currentRole => currentRole.Equals(r, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (nonExistingRolesToRemove != null && nonExistingRolesToRemove.Any())
            {
                throw new ArgumentException($"The following roles do not exist for the user and cannot be removed: {string.Join(", ", nonExistingRolesToRemove)}");
            }

            // Add roles
            if (rolesToAdd?.Any() == true)
            {
                var result = await _userManager.AddToRolesAsync(user, rolesToAdd);
                
                if (!result.Succeeded)
                {
                    var error = $"Failed to add roles: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                    _logger.LogError(error);
                    errors.Add(error);
                }
                else
                {
                    _logger.LogInformation($"Successfully added roles [{string.Join(", ", rolesToAdd)}] to user: {userId}");
                }
            }

            // Remove roles
            if (rolesToRemove?.Any() == true)
            {
                var result = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                
                if (!result.Succeeded)
                {
                    var error = $"Failed to remove roles: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                    _logger.LogError(error);
                    errors.Add(error);
                }
                else
                {
                    _logger.LogInformation($"Successfully removed roles [{string.Join(", ", rolesToRemove)}] from user: {userId}");
                }
            }

            if (errors.Any())
            {
                throw new InvalidOperationException($"Some operations failed: {string.Join("; ", errors)}");
            }

            _logger.LogInformation($"Successfully managed roles for user: {userId}");
        }

        #region Multi factor authentication

        public async Task<MfaStatusModel> GetMfaStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found when fetching MFA status: {userId}");
                throw new InvalidOperationException("User not found");
            }
            return await BuildMfaStatus(user);
        }

        public async Task<MfaSetupResult> StartMfaSetup(string userId, MfaMethods method)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found when starting MFA setup: {userId}");
                throw new InvalidOperationException("User not found");
            }

            if (method == MfaMethods.Authenticator)
            {
                var key = await _userManager.GetAuthenticatorKeyAsync(user);
                if (string.IsNullOrEmpty(key))
                {
                    await _userManager.ResetAuthenticatorKeyAsync(user);
                    key = await _userManager.GetAuthenticatorKeyAsync(user);
                }
                if (string.IsNullOrEmpty(key))
                {
                    throw new InvalidOperationException("Failed to generate an authenticator key");
                }
                return new MfaSetupResult
                {
                    Method = MfaMethods.Authenticator,
                    SharedKey = key,
                    AuthenticatorUri = BuildAuthenticatorUri(user.Email ?? user.UserName ?? string.Empty, key)
                };
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                throw new InvalidOperationException("User does not have an email address");
            }
            await EnqueueOrSendMfaEmailCode(user);
            return new MfaSetupResult
            {
                Method = MfaMethods.Email,
                CodeSent = true
            };
        }

        public async Task<MfaStatusModel> ConfirmMfaSetup(string userId, MfaConfirmModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found when confirming MFA setup: {userId}");
                throw new InvalidOperationException("User not found");
            }

            var provider = GetTokenProvider(model.Method);
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, provider, NormalizeCode(model.Code));
            if (!isValid)
            {
                _logger.LogWarning($"Invalid MFA code when confirming method {model.Method} for user {userId}");
                throw new InvalidOperationException("The provided code is invalid or has expired.");
            }

            var setResult = await SetMfaMethodEnabled(user, model.Method, true);
            if (!setResult)
            {
                throw new InvalidOperationException("Failed to enable MFA method");
            }

            await SyncTwoFactorEnabledFlag(user);
            _logger.LogInformation($"MFA method {model.Method} enabled for user {userId}");
            return await BuildMfaStatus(user);
        }

        public async Task<MfaStatusModel> DisableMfaMethod(string userId, MfaDisableModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found when disabling MFA method: {userId}");
                throw new InvalidOperationException("User not found");
            }

            if (!await IsMfaMethodEnabled(user, model.Method))
            {
                return await BuildMfaStatus(user);
            }

            // If a code is supplied, verify it. This allows the UI to require a confirmation code before disabling.
            if (!string.IsNullOrEmpty(model.Code))
            {
                var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, GetTokenProvider(model.Method), NormalizeCode(model.Code));
                if (!isValid)
                {
                    throw new InvalidOperationException("The provided code is invalid or has expired.");
                }
            }

            var removeResult = await SetMfaMethodEnabled(user, model.Method, false);
            if (!removeResult)
            {
                throw new InvalidOperationException("Failed to disable MFA method");
            }

            if (model.Method == MfaMethods.Authenticator)
            {
                // Invalidate the shared key so that a new one is generated on the next setup.
                await _userManager.ResetAuthenticatorKeyAsync(user);
            }

            await SyncTwoFactorEnabledFlag(user);
            _logger.LogInformation($"MFA method {model.Method} disabled for user {userId}");
            return await BuildMfaStatus(user);
        }

        private static string GetTokenProvider(MfaMethods method) => method switch
        {
            MfaMethods.Email => TokenOptions.DefaultEmailProvider,
            MfaMethods.Authenticator => TokenOptions.DefaultAuthenticatorProvider,
            _ => throw new ArgumentOutOfRangeException(nameof(method))
        };

        private static string NormalizeCode(string code) => (code ?? string.Empty).Replace(" ", string.Empty).Replace("-", string.Empty);

        /// <summary>
        /// Enables or disables a single MFA method for the user. The row is kept when the method is
        /// disabled so that the setup history stays visible.
        /// </summary>
        private async Task<bool> SetMfaMethodEnabled(ApplicationUser user, MfaMethods method, bool enabled)
        {
            var utcNow = _dateService.LocalToUtc(_dateService.CurrentTime());
            var existing = await _applicationDbContext.UserMfaMethods
                .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Method == method);

            if (existing == null)
            {
                if (!enabled)
                {
                    return true;
                }
                _applicationDbContext.UserMfaMethods.Add(new UserMfaMethod
                {
                    UserId = user.Id,
                    Method = method,
                    Enabled = true,
                    ConfirmedUtc = utcNow
                });
            }
            else
            {
                existing.Enabled = enabled;
                if (enabled)
                {
                    existing.ConfirmedUtc = utcNow;
                }
                else
                {
                    existing.DisabledUtc = utcNow;
                }
            }

            await _applicationDbContext.SaveChangesAsync();
            return true;
        }

        private async Task<List<MfaMethods>> GetEnabledMfaMethods(string userId)
        {
            return await _applicationDbContext.UserMfaMethods
                .Where(x => x.UserId == userId && x.Enabled)
                .OrderBy(x => x.Method)
                .Select(x => x.Method)
                .ToListAsync();
        }

        private async Task<bool> IsMfaMethodEnabled(ApplicationUser user, MfaMethods method)
        {
            return await _applicationDbContext.UserMfaMethods
                .AnyAsync(x => x.UserId == user.Id && x.Method == method && x.Enabled);
        }

        private async Task<MfaStatusModel> BuildMfaStatus(ApplicationUser user)
        {
            var enabledMethods = await GetEnabledMfaMethods(user.Id);
            return new MfaStatusModel
            {
                EmailEnabled = enabledMethods.Contains(MfaMethods.Email),
                AuthenticatorEnabled = enabledMethods.Contains(MfaMethods.Authenticator),
                EnabledMethods = enabledMethods,
                TwoFactorEnabled = enabledMethods.Any()
            };
        }

        private async Task SyncTwoFactorEnabledFlag(ApplicationUser user)
        {
            var status = await BuildMfaStatus(user);
            if (user.TwoFactorEnabled != status.TwoFactorEnabled)
            {
                var result = await _userManager.SetTwoFactorEnabledAsync(user, status.TwoFactorEnabled);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to update two factor state: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private string BuildAuthenticatorUri(string account, string sharedKey)
        {
            var issuerName = string.IsNullOrWhiteSpace(_applicationSettings.MfaIssuer)
                ? ApplicationConstants.MfaIssuer
                : _applicationSettings.MfaIssuer.Trim();
            var issuer = Uri.EscapeDataString(issuerName);
            return $"otpauth://totp/{issuer}:{Uri.EscapeDataString(account)}?secret={sharedKey}&issuer={issuer}&digits=6";
        }

        /// <summary>
        /// Generates and sends an MFA code to the user's email address.
        /// Public so that it can be executed as a Hangfire background job.
        /// </summary>
        public async Task SendMfaEmailCode(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found when sending an MFA code: {userId}");
                throw new InvalidOperationException("User not found");
            }
            await SendMfaEmailCode(user);
        }

        /// <summary>
        /// Sends the MFA code in the background when Hangfire is available, otherwise sends it
        /// directly. The code itself is always generated by the job so that it is never stored
        /// in the Hangfire job data.
        /// </summary>
        private async Task EnqueueOrSendMfaEmailCode(ApplicationUser user)
        {
            if (string.IsNullOrEmpty(user.Email))
            {
                throw new InvalidOperationException("User does not have an email address");
            }

            if (_hangfireJobService.IsAvailable)
            {
                var userId = user.Id;
                _hangfireJobService.Enqueue<IUserAuthService>(service => service.SendMfaEmailCode(userId));
                _logger.LogInformation($"MFA code email enqueued for user {user.Id}");
                return;
            }

            await SendMfaEmailCode(user);
        }

        private async Task SendMfaEmailCode(ApplicationUser user)
        {
            if (string.IsNullOrEmpty(user.Email))
            {
                throw new InvalidOperationException("User does not have an email address");
            }

            var code = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
            var emailTemplate = await _emailRepository.GetEmailTemplate(EmailTemplateTypes.MfaCode);

            var subject = string.IsNullOrEmpty(emailTemplate?.Title) ? "Your verification code" : emailTemplate.Title;
            var htmlContent = string.IsNullOrEmpty(emailTemplate?.Message)
                ? $"<p>Your verification code is <strong>{ApplicationConstants.EmailTemplateMfaCodeKey}</strong>.</p>" +
                  $"<p>The code is valid for a few minutes. If you did not request this code, you can ignore this message.</p>"
                : emailTemplate.Message;

            await _emailClient.SendEmailAsync(new SendEmailOptions
            {
                ToAddresses = new List<string> { user.Email },
                Subject = subject,
                HtmlContent = htmlContent,
                ReplaceTokens = new Dictionary<string, string>
                {
                    { ApplicationConstants.EmailTemplateMfaCodeKey, code },
                    { ApplicationConstants.QueuedMessageApplicationBaseUrlKey, _applicationSettings.BaseUrl ?? string.Empty },
                }
            });
            _logger.LogInformation($"MFA code email sent to user {user.Id}");
        }

        /// <summary>
        /// Sends the email code automatically when email is the only enabled method,
        /// so that the user does not have to request it separately.
        /// </summary>
        private async Task SendMfaEmailCodeIfOnlyMethod(ApplicationUser user, MfaStatusModel status)
        {
            if (status.EmailEnabled && !status.AuthenticatorEnabled)
            {
                try
                {
                    await EnqueueOrSendMfaEmailCode(user);
                }
                catch (Exception ex)
                {
                    // The user can request the code again from the challenge endpoint.
                    _logger.LogError(ex, $"Failed to send MFA code email to user {user.Id}");
                }
            }
        }

        public async Task<MfaChallengeModel> GetPendingMfaChallenge()
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return new MfaChallengeModel { ChallengePending = false };
            }
            var status = await BuildMfaStatus(user);
            return new MfaChallengeModel
            {
                ChallengePending = status.TwoFactorEnabled,
                AvailableMethods = status.EnabledMethods,
                LoginProvider = await GetPendingLoginProvider(),
                MaskedEmail = MaskEmail(user.Email)
            };
        }

        public async Task SendPendingMfaEmailCode()
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new UnauthorizedAccessException("No pending two factor authentication challenge");
            }
            if (!await IsMfaMethodEnabled(user, MfaMethods.Email))
            {
                throw new InvalidOperationException("Email is not enabled as a two factor authentication method for this user.");
            }
            await EnqueueOrSendMfaEmailCode(user);
        }

        public async Task<LoginResultModel> CompleteTwoFactorLogin(MfaLoginModel model)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new UnauthorizedAccessException("No pending two factor authentication challenge");
            }

            if (!await IsMfaMethodEnabled(user, model.Method))
            {
                throw new InvalidOperationException($"Method {model.Method} is not enabled for this user.");
            }

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, GetTokenProvider(model.Method), NormalizeCode(model.Code));
            if (!isValid)
            {
                _logger.LogWarning($"Invalid two factor code supplied for user {user.Id} using method {model.Method}");
                throw new UnauthorizedAccessException("The provided code is invalid or has expired.");
            }

            var claims = await GetCalculatedClaims(user);
            var loginProvider = await GetPendingLoginProvider();
            if (!string.IsNullOrEmpty(loginProvider))
            {
                var upn = await _userManager.GetAuthenticationTokenAsync(
                    user,
                    ApplicationConstants.MfaTokenProviderName,
                    ApplicationConstants.MfaPendingUpnTokenName);
                claims.Add(new Claim(ApplicationConstants.ExternalLoginProviderClaim, loginProvider));
                claims.Add(new Claim(ClaimTypes.Upn, upn ?? string.Empty));
                await _userManager.RemoveAuthenticationTokenAsync(
                    user,
                    ApplicationConstants.MfaTokenProviderName,
                    ApplicationConstants.MfaPendingUpnTokenName);
            }

            await _signInManager.SignInWithClaimsAsync(user, model.Persistent, claims);
            await _signInManager.Context.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
            _logger.LogInformation($"Two factor login completed for user {user.Id} using method {model.Method}");
            return new LoginResultModel { Success = true };
        }

        private async Task<string?> GetPendingLoginProvider()
        {
            var authenticateResult = await _signInManager.Context.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
            var provider = authenticateResult?.Principal?.FindFirstValue(ClaimTypes.AuthenticationMethod);
            return string.IsNullOrEmpty(provider) ? null : provider;
        }

        private static string? MaskEmail(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }
            var atIndex = email.IndexOf('@');
            if (atIndex <= 0)
            {
                return null;
            }
            var name = email.Substring(0, atIndex);
            var domain = email.Substring(atIndex);
            var visible = name.Length <= 2 ? name.Substring(0, 1) : name.Substring(0, 2);
            return $"{visible}{new string('*', Math.Max(1, name.Length - visible.Length))}{domain}";
        }

        #endregion

        /// <summary>

        /// Get calculated claims. Each location gives a claim to devices in the location. 
        /// Each device gives a claim to each sensor attached to the device.
        /// Users with Viewer role get claims to all locations.
        /// Returns consolidated claims (multiple identifiers in single claim separated by semicolon).
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<List<Claim>> GetCalculatedClaims(ApplicationUser user)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            // Check if user has Viewer role
            var isViewer = userRoles.Any(r => r.Equals(GlobalRoles.Viewer.ToString(), StringComparison.OrdinalIgnoreCase));

            List<Guid> locationIdentifiersAsClaims;

            // If user is a Viewer, grant access to all locations
            if (isViewer)
            {
                locationIdentifiersAsClaims = await _measurementDbContext.Locations
                    .Select(l => l.Identifier)
                    .ToListAsync();
            }
            else
            {
                // For non-Viewer users, get locations from their claims
                locationIdentifiersAsClaims = claims
                    .Where(x => x.Type == EntityRoles.Location.ToString())
                    .Select(x => Guid.TryParse(x.Value, out var guid) ? (Guid?)guid : null)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToList();
            }

            var existingLocationIdsInClaims = claims
                .Where(x => x.Type == EntityRoles.Location.ToString())
                .Select(x => Guid.TryParse(x.Value, out var guid) ? (Guid?)guid : null)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();

            var existingSensorIdsInClaims = claims
                .Where(x => x.Type == EntityRoles.Sensor.ToString())
                .Select(x => Guid.TryParse(x.Value, out var guid) ? (Guid?)guid : null)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();

            var deviceIdentifiersAsClaims = claims
                .Where(x => x.Type == EntityRoles.Device.ToString())
                .Select(x => Guid.TryParse(x.Value, out var guid) ? (Guid?)guid : null)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();

            var deviceIdentifiersMatchingsLocations = (await _deviceRepository.GetDevices(new GetDevicesModel() { LocationIdentifiers = locationIdentifiersAsClaims })).Select(x => x.Identifier).ToList();

            var deviceIdentifiers = new List<Guid>(deviceIdentifiersAsClaims);
            deviceIdentifiers.AddRange(deviceIdentifiersMatchingsLocations);

            var additionalClaims = new List<Claim>();
            var sensorIdentifiersMatchingDevices = (await _deviceRepository.GetSensors(new GetSensorsModel()
            {
                DevicesModel = new GetDevicesModel()
                {
                    Identifiers = deviceIdentifiers
                }
            })).Select(x => x.Identifier).ToList();

            additionalClaims.AddRange(sensorIdentifiersMatchingDevices
                .Where(x => !existingSensorIdsInClaims.Contains(x))
                .Distinct()
                .Select(x => new Claim(EntityRoles.Sensor.ToString(), x.ToString())));
            additionalClaims.AddRange(deviceIdentifiersMatchingsLocations
                .Where(x => !deviceIdentifiersAsClaims.Contains(x))
                .Distinct()
                .Select(x => new Claim(EntityRoles.Device.ToString(), x.ToString())));

            // Add location claims that aren't already in user's existing claims
            additionalClaims.AddRange(locationIdentifiersAsClaims
                .Where(x => !existingLocationIdsInClaims.Contains(x))
                .Distinct()
                .Select(x => new Claim(EntityRoles.Location.ToString(), x.ToString())));

            var groupedClaims = ClaimsConsolidationHelper.ConsolidateClaimsList(additionalClaims);

            return groupedClaims;
        }
    }
}
