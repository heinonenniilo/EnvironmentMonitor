using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Infrastructure.Data.Migrations.Application;
using EnvironmentMonitor.Infrastructure.Identity;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.WebApi.Attributes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using LoginModel = EnvironmentMonitor.Domain.Models.LoginModel;
using ExternalLoginModel = EnvironmentMonitor.Domain.Models.ExternalLoginModel;
using ForgotPasswordRequest = EnvironmentMonitor.Application.DTOs.ForgotPasswordRequest;
using ResetPasswordRequest = EnvironmentMonitor.Application.DTOs.ResetPasswordRequest;
using ChangePasswordRequest = EnvironmentMonitor.Application.DTOs.ChangePasswordRequest;
using ForgotPasswordModel = EnvironmentMonitor.Domain.Models.ForgotPasswordModel;
using ResetPasswordModel = EnvironmentMonitor.Domain.Models.ResetPasswordModel;
using ChangePasswordModel = EnvironmentMonitor.Domain.Models.ChangePasswordModel;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IUserService _userService;

        public AuthenticationController(
            ILogger<AuthenticationController> logger,
            SignInManager<ApplicationUser> signInManager,
            IUserService userService)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            await _userService.RegisterUser(new RegisterUserModel() 
            { 
                Email = request.Email, 
                Password = request.Password,
                ConfirmPassword = request.Password
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
            await _userService.ForgotPassword(new ForgotPasswordModel
            {
                Email = request.Email,
                BaseUrl = "", // Will be set by UserService,
                Enqueue = false // TODO Add to true once data protection keys are stored in the same location
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
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            await _userService.Login(new Domain.Models.LoginModel()
            {
                UserName = request.UserName,
                Password = request.Password,
                Persistent = request.Persistent
            });
            return Ok(new { Message = "Login successful!" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogOut()
        {
            var u = User;
            u.FindFirstValue(ClaimTypes.Email);
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "Logged out successfully." });
        }

        [HttpGet("info")]
        public ActionResult<UserDto?> UserInfo()
        {
            if (User == null || User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Ok(null);
            }
            var roles = User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value);
            
            // Check for external login provider claim
            var externalProviderClaim = User.Claims.FirstOrDefault(c => c.Type == ApplicationConstants.ExternalLoginProviderClaim);
            string? authProvider = externalProviderClaim?.Value;

            return Ok(new UserDto()
            {
                Email = User.FindFirstValue(ClaimTypes.Email),
                Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Roles = roles.ToList(),
                AuthenticationProvider = authProvider
            });
        }

        [HttpGet("google")]
        public IActionResult GoogleLogin([FromQuery] bool persistent = false)
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Authentication", new { returnUrl = "/", persistent });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(string returnUrl = "/", bool persistent = false)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
            {
                _logger.LogWarning($"Not authenticated at GoogleCallback");
                return Unauthorized(new { Message = "Authentication failed." });
            }
            await _userService.ExternalLogin(new ExternalLoginModel()
            {
                Persistent = persistent
            });
            return Redirect(returnUrl ?? "/");
        }
    }
}
