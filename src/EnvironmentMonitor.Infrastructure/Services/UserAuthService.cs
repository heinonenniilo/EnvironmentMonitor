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

        public UserAuthService(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IDeviceRepository deviceRepository,
            ILogger<UserAuthService> logger,
            IEmailClient emailClient,
            IEmailRepository emailRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _deviceRepository = deviceRepository;
            _logger = logger;
            _emailClient = emailClient;
            _emailRepository = emailRepository;
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

        public async Task LoginWithExternalProvider(ExternalLoginModel model)
        {
            string loginProvider;
            string providerKey;
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                throw new InvalidOperationException();
            }
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException("Failed to fetch email");
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
                throw new InvalidOperationException();
            }

            var user = await _userManager.FindByLoginAsync(loginProvider, providerKey);
            if (user != null)
            {
                var additionalClaims = await GetCalculatedClaims(user);
                additionalClaims.Add(new Claim(ApplicationConstants.ExternalLoginProviderClaim, loginProvider));
                await _signInManager.SignInWithClaimsAsync(user, model.Persistent, additionalClaims);
            }
            else
            {
                user = new ApplicationUser { UserName = model.UserName ?? email, Email = email };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }

                var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(
                    loginProvider,
                    providerKey, providerKey
                    ));

                if (!addLoginResult.Succeeded)
                {
                    throw new InvalidOperationException("Failed to add login");
                }
                var additionalClaims = new List<Claim>
                {
                    new Claim(ApplicationConstants.ExternalLoginProviderClaim, loginProvider)
                };
                await _signInManager.SignInWithClaimsAsync(user, model.Persistent, additionalClaims);
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
                _logger.LogInformation($"Forgot password requested for non-existent or unconfirmed email: {model.Email}");
                return;
            }

            // Generate password reset token and construct reset URL
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);           
            var queryParams = new StringBuilder();
            queryParams.Append($"?email={Uri.EscapeDataString(user.Email!)}");
            queryParams.Append($"&token={Uri.EscapeDataString(token)}");            
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

        /// <summary>
        /// Get calculated claims. Each location gives a claim to devices in the location. Each device gives a claim to each sensor attached to the device.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<List<Claim>> GetCalculatedClaims(ApplicationUser user)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            
            var locationIdentifiersAsClaims = claims
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
            return claimsToReturn;
        }
    }
}
