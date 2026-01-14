using Azure.Core;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Identity;
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

        public UserAuthService(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IDeviceRepository deviceRepository,
            ILogger<UserAuthService> logger,
            IEmailClient emailClient,
            IEmailRepository emailRepository,
            MeasurementDbContext measurementDbContext,
            ApplicationDbContext applicationDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _deviceRepository = deviceRepository;
            _logger = logger;
            _emailClient = emailClient;
            _emailRepository = emailRepository;
            _measurementDbContext = measurementDbContext;
            _applicationDbContext = applicationDbContext;
        }

        public async Task Login(LoginModel model)
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
            var calculatedClaims = await GetCalculatedClaims(user);
            await _signInManager.SignInWithClaimsAsync(user, model.Persistent, calculatedClaims);
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
                        Errors = [$"Failed to link the user with external login provider. (${loginProvider})"],
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
                throw new DuplicateEntityException($"User with email '{model.Email}' already exists");
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
            
            var confirmationUrl = model.BaseUrl + queryParams.ToString();
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
                Subject = emailTemplate.Title ,
                HtmlContent = emailTemplate.Message,
                ReplaceTokens = new Dictionary<string, string>
                {
                    { ApplicationConstants.EmailTemplateConfirmationLinkKey, confirmationUrl },
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
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogError($"Forgot password requested for non-existent or unconfirmed email: {model.Email}");
                throw new InvalidOperationException($"Forgot password requested for non-existent or unconfirmed email: {model.Email}");
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);           
            var queryParams = new StringBuilder();
            queryParams.Append($"?token={Uri.EscapeDataString(token)}");        
            var resetUrl = model.BaseUrl + queryParams.ToString();

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

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = await _userManager.GetClaimsAsync(user);
                var logins = await _userManager.GetLoginsAsync(user);

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

        /// <summary>
        /// Get calculated claims. Each location gives a claim to devices in the location. Each device gives a claim to each sensor attached to the device.
        /// Users with Viewer role get claims to all locations.
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

            var claimsToReturn = new List<Claim>();
            var sensorIdentifiersMatchingDevices = (await _deviceRepository.GetSensors(new GetSensorsModel()
            {
                DevicesModel = new GetDevicesModel()
                {
                    Identifiers = deviceIdentifiers
                }
            })).Select(x => x.Identifier).ToList();

            claimsToReturn.AddRange(sensorIdentifiersMatchingDevices
                .Where(x => !existingSensorIdsInClaims.Contains(x))
                .Distinct()
                .Select(x => new Claim(EntityRoles.Sensor.ToString(), x.ToString())));
            claimsToReturn.AddRange(deviceIdentifiersMatchingsLocations
                .Where(x => !deviceIdentifiersAsClaims.Contains(x))
                .Distinct()
                .Select(x => new Claim(EntityRoles.Device.ToString(), x.ToString())));
            
            // Add location claims that aren't already in user's existing claims
            claimsToReturn.AddRange(locationIdentifiersAsClaims
                .Where(x => !existingLocationIdsInClaims.Contains(x))
                .Distinct()
                .Select(x => new Claim(EntityRoles.Location.ToString(), x.ToString())));
            
            return claimsToReturn;
        }
    }
}
