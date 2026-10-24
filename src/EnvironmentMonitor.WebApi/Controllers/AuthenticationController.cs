using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Infrastructure.Identity;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LoginModel = EnvironmentMonitor.Domain.Models.LoginModel;
using ExternalLoginModel = EnvironmentMonitor.Domain.Models.ExternalLoginModel;
using ForgotPasswordRequest = EnvironmentMonitor.Application.DTOs.ForgotPasswordRequest;
using ResetPasswordRequest = EnvironmentMonitor.Application.DTOs.ResetPasswordRequest;
using ChangePasswordRequest = EnvironmentMonitor.Application.DTOs.ChangePasswordRequest;
using ForgotPasswordModel = EnvironmentMonitor.Domain.Models.ForgotPasswordModel;
using ResetPasswordModel = EnvironmentMonitor.Domain.Models.ResetPasswordModel;
using ChangePasswordModel = EnvironmentMonitor.Domain.Models.ChangePasswordModel;
using AspNet.Security.OAuth.GitHub; // GitHub OAuth

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IUserService _userService;
        private readonly GitHubSettings _githubSettings;


        private const string LoginInfoRoute = "/login/info";
        private const string MfaChallengeRoute = "/login/mfa";

        public AuthenticationController(
            ILogger<AuthenticationController> logger,
            SignInManager<ApplicationUser> signInManager,
            IUserService userService,
            GitHubSettings githubSettings)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userService = userService;
            _githubSettings = githubSettings;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            await _userService.RegisterUser(new RegisterUserModel() 
            { 
                Email = request.Email, 
                Password = request.Password,
                ConfirmPassword = request.Password,
                BaseUrl = baseUrl
            });
            return Ok(new { Message = "User registered successfully. Please check your email to confirm your account." });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return Redirect("/email-confirmation?status=invalid");
            }

            var result = await _userService.ConfirmEmail(userId, token);

            if (result)
            {
                return Redirect("/email-confirmation?status=success");
            }

            throw new InvalidOperationException();
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                await _userService.ChangePassword(new ChangePasswordModel
                {
                    CurrentPassword = request.CurrentPassword,
                    NewPassword = request.NewPassword
                });
                
                return Ok(new { Message = "Password changed successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}"; 

            await _userService.ForgotPassword(new ForgotPasswordModel
            {
                Email = request.Email,
                BaseUrl = baseUrl,
                Enqueue = true
            });

            return Ok(new { Message = "If the email exists, a password reset link has been sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _userService.ResetPassword(new ResetPasswordModel
            {
                Email = request.Email,
                Token = request.Token,
                NewPassword = request.NewPassword
            });
            
            if (result)
            {
                return Ok(new { Message = "Password reset successfully. You can now log in with your new password." });
            }
            
            return BadRequest(new { Message = "Password reset failed. The link may be invalid or expired." });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResultDto>> Login([FromBody] LoginModel request)
        {
            var result = await _userService.Login(new Domain.Models.LoginModel()
            {
                UserName = request.UserName,
                Password = request.Password,
                Persistent = request.Persistent
            });
            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            _userService.ClearAuthInfo();
            return Ok(new { Message = "Logged out successfully." });
        }

        [HttpGet("info")]
        public async Task<ActionResult<UserDto?>> UserInfo()
        {
            if (User == null || User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Ok(null);
            }
            var roles = User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value);

            // Check for external login provider claim
            var externalProviderClaim = User.Claims.FirstOrDefault(c => c.Type == ApplicationConstants.ExternalLoginProviderClaim);
            string? authProvider = externalProviderClaim?.Value;

            // Get UPN (User Principal Name) - check only ClaimTypes.Upn and preferred_username
            string? upn = User.FindFirstValue(ClaimTypes.Upn);

            return Ok(new UserDto()
            {
                Email = User.FindFirstValue(ClaimTypes.Email),
                Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Roles = roles.ToList(),
                EmailConfirmed = await _userService.IsEmailConfirmed(),
                AuthenticationProvider = authProvider,
                UpnExternal = upn,
                Mfa = await _userService.GetMfaStatus()
            });
        }

        [HttpGet("{provider}-callback")]
        public async Task<IActionResult> ExternalCallback(string provider, string returnUrl = "/", bool persistent = false)
        {
            // Determine the authentication scheme based on the provider
            string authenticationScheme = provider.ToLower() switch
            {
                "google" => GoogleDefaults.AuthenticationScheme,
                "microsoft" => MicrosoftAccountDefaults.AuthenticationScheme,
                "github" => GitHubAuthenticationDefaults.AuthenticationScheme,
                _ => throw new ArgumentException("Unsupported provider")
            };
            var authenticateResult = await HttpContext.AuthenticateAsync(authenticationScheme);
            if (!authenticateResult.Succeeded)
            {
                _logger.LogWarning($"Not authenticated at {provider}-callback");
                return Redirect(LoginInfoRoute);
            }
            var result = await _userService.ExternalLogin(new ExternalLoginModel()
            {
                Persistent = persistent
            });
            var returnUrlToSet = returnUrl ?? "/";

            if (!Url.IsLocalUrl(returnUrlToSet))
            {
                _logger.LogWarning($"Non local return URL observed: {returnUrlToSet}");
                returnUrlToSet = "/";
            }

            if (result.RequiresTwoFactor)
            {
                _logger.LogInformation($"Two factor authentication required after {provider} login.");
                return Redirect($"{MfaChallengeRoute}?returnUrl={Uri.EscapeDataString(returnUrlToSet)}&persistent={persistent}");
            }

            if (!result.Success)
            {
                _logger.LogWarning($"External login failed at {provider}-callback with error code: {result.ErrorCode}");
                return Redirect(LoginInfoRoute);
            }
            return Redirect(returnUrlToSet);
        }

        [HttpGet("google")]
        public IActionResult GoogleLogin([FromQuery] bool persistent = false)
        {
            var redirectUrl = Url.Action(nameof(ExternalCallback), "Authentication", new { provider = "google", returnUrl = "/", persistent });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("microsoft")]
        public IActionResult MicrosoftLogin([FromQuery] bool persistent = false)
        {
            var redirectUrl = Url.Action(nameof(ExternalCallback), "Authentication", new { provider = "microsoft", returnUrl = "/", persistent });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(MicrosoftAccountDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
        }

        // GitHub OAuth endpoints
        [HttpGet("github")]
        public IActionResult GitHubLogin([FromQuery] bool persistent = false)
        {
            if (UriUtils.IsValidHttpUrl(_githubSettings.GitHubHost))
            {
                var currentHost = $"{Request.Scheme}://{Request.Host}";
                var configuredHost = _githubSettings.GitHubHost.TrimEnd('/');

                var currentUri = new Uri(currentHost);
                var configuredUri = new Uri(configuredHost);
                // If the current host is different from the configured GitHub host, redirect
                if (!currentUri.Host.Equals(configuredUri.Host, StringComparison.OrdinalIgnoreCase) ||
                    currentUri.Port != configuredUri.Port)
                {
                    var redirectToGitHubHost = $"{configuredHost}/api/authentication/github?persistent={persistent}";
                    _logger.LogInformation($"Redirecting GitHub OAuth request from {currentHost} to {redirectToGitHubHost}");
                    return Redirect(redirectToGitHubHost);
                }
            }
            else if (!string.IsNullOrEmpty(_githubSettings.GitHubHost))
            {
                _logger.LogWarning($"GitHubHost is configured but is not a valid URL: {_githubSettings.GitHubHost}");
            }
            
            var redirectUrl = Url.Action(nameof(ExternalCallback), "Authentication", new { provider = "github", returnUrl = "/", persistent });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(GitHubAuthenticationDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GitHubAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpDelete("")]
        [Authorize]
        public async Task<IActionResult> DeleteOwnUser()
        {
            await _userService.DeleteOwnUser();
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "User deleted" });
        }

        [HttpGet("auth-info")]
        public ActionResult<AuthInfoCookie?> GetAuthInfo()
        {
            var authInfo = _userService.GetAuthInfo();
            return Ok(authInfo);
        }

        [HttpDelete("auth-info")]
        public IActionResult ClearAuthInfo()
        {
            _userService.ClearAuthInfo();
            return Ok(new { Message = "Auth info cleared" });
        }

        #region Multi factor authentication

        /// <summary>
        /// Sends an email confirmation code to the authenticated user's email address.
        /// Mainly for users created through an external login, whose email is not confirmed.
        /// </summary>
        [HttpPost("email/send-confirmation-code")]
        [Authorize]
        public async Task<IActionResult> SendEmailConfirmationCode()
        {
            await _userService.SendEmailConfirmationCode();
            return Ok(new { Message = "Confirmation code sent." });
        }

        /// <summary>
        /// Confirms the authenticated user's email address with a code sent by send-confirmation-code.
        /// </summary>
        [HttpPost("email/confirm-code")]
        [Authorize]
        public async Task<IActionResult> ConfirmEmailWithCode([FromBody] ConfirmEmailCodeRequest request)
        {
            var result = await _userService.ConfirmEmailWithCode(request);
            if (!result)
            {
                return BadRequest(new { Message = "The provided code is invalid or has expired." });
            }
            return Ok(new { Message = "Email confirmed.", EmailConfirmed = true });
        }

        /// <summary>
        /// Returns the MFA methods the authenticated user has enabled.
        /// </summary>
        [HttpGet("mfa")]
        [Authorize]
        public async Task<ActionResult<MfaStatusDto>> GetMfaStatus()
        {
            return Ok(await _userService.GetMfaStatus());
        }

        /// <summary>
        /// Starts the setup of an MFA method. For the authenticator app the shared key and the
        /// otpauth uri (for the QR code) are returned, for email a code is sent to the user's email address.
        /// </summary>
        [HttpPost("mfa/setup")]
        [Authorize]
        public async Task<ActionResult<MfaSetupDto>> StartMfaSetup([FromBody] MfaSetupRequest request)
        {
            return Ok(await _userService.StartMfaSetup(request));
        }

        /// <summary>
        /// Confirms and enables an MFA method with a code from the authenticator app or from email.
        /// </summary>
        [HttpPost("mfa/confirm")]
        [Authorize]
        public async Task<ActionResult<MfaStatusDto>> ConfirmMfaSetup([FromBody] MfaConfirmRequest request)
        {
            return Ok(await _userService.ConfirmMfaSetup(request));
        }

        /// <summary>
        /// Disables a single MFA method. A code can optionally be supplied and is verified when present.
        /// </summary>
        [HttpPost("mfa/disable")]
        [Authorize]
        public async Task<ActionResult<MfaStatusDto>> DisableMfaMethod([FromBody] MfaDisableRequest request)
        {
            return Ok(await _userService.DisableMfaMethod(request));
        }

        /// <summary>
        /// Returns the pending two factor challenge for a user who has passed the first login step.
        /// </summary>
        [HttpGet("mfa/challenge")]
        public async Task<ActionResult<MfaChallengeDto>> GetMfaChallenge()
        {
            return Ok(await _userService.GetPendingMfaChallenge());
        }

        /// <summary>
        /// Sends (or re-sends) the email code for a pending two factor challenge.
        /// </summary>
        [HttpPost("mfa/challenge/send-email-code")]
        public async Task<IActionResult> SendMfaChallengeEmailCode()
        {
            await _userService.SendPendingMfaEmailCode();
            return Ok(new { Message = "Verification code sent." });
        }

        /// <summary>
        /// Completes the login by verifying a code from the selected MFA method.
        /// </summary>
        [HttpPost("mfa/login")]
        public async Task<ActionResult<LoginResultDto>> CompleteMfaLogin([FromBody] MfaLoginRequest request)
        {
            return Ok(await _userService.CompleteTwoFactorLogin(request));
        }

        #endregion
    }
}
