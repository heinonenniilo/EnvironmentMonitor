using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
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
            // Build the base URL from the current request
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            
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
                return BadRequest(new { Message = "Invalid confirmation link." });
            }

            var result = await _userService.ConfirmEmail(userId, token);
            
            if (result)
            {
                return Ok(new { Message = "Email confirmed successfully! You can now log in." });
            }
            
            return BadRequest(new { Message = "Email confirmation failed. The link may be invalid or expired." });
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

            return Ok(new UserDto()
            {
                Email = User.FindFirstValue(ClaimTypes.Email),
                Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Roles = roles.ToList(),
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
