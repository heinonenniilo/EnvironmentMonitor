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
        [ApiKeyRequired]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            await _userService.RegisterUser(new RegisterUserModel() { Email = request.Email, Password = request.Password });
            return Ok(new { Message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            await _userService.Login(new Domain.Models.LoginModel()
            {
                UserName = request.Email,
                Password = request.Password,
                Persistent = true
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
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Authentication");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(string returnUrl = "/")
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
            {
                _logger.LogWarning($"Not authenticated at GoogleCallback");
                return Unauthorized(new { Message = "Authentication failed." });
            }
            await _userService.ExternalLogin(new ExternalLoginModel()
            {
                Persistent = true
            });
            return Redirect(returnUrl ?? "/");
        }
    }
}
