using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Infrastructure.Identity;
using EnvironmentMonitor.WebApi.Attributes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthenticationController(ILogger<AuthenticationController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IHttpContextAccessor contextAccessor)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _contextAccessor = contextAccessor;
        }

        [HttpPost("register")]
        [ApiKeyRequired]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _userManager.FindByEmailAsync(request.Email) != null)
                return BadRequest(new { Message = "Email already exists." });

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);
            _logger.LogInformation($"User with email {request.Email} created.");
            return Ok(new { Message = "User registered successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, true, false); // Verify
            if (!result.Succeeded)
                return Unauthorized(new { Message = "Invalid username or password." });

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
        public ActionResult<UserDto> UserInfo()
        {
            if (User == null || User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return NotFound();
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
            var properties = _signInManager.ConfigureExternalAuthenticationProperties
                (
                GoogleDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(string returnUrl = "/")
        {      
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
            {
                return Unauthorized(new { Message = "Authentication failed." });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true);
            if (signInResult.Succeeded)
            {
                return Redirect(returnUrl ?? "/");
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (email != null)
                {
                    var user = new ApplicationUser { UserName = email, Email = email };
                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddLoginAsync(user, info);
                        if (result.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: true);
                            return Redirect(returnUrl ?? "/");
                        }
                    }
                }
                return BadRequest(new { message = "Failed to login with external provider." });
            }
        }
    }
}
