using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using EnvironmentMonitor.Domain.Enums;

namespace EnvironmentMonitor.WebApi.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ApiKey";
        public string Scheme => DefaultScheme;
        public string ApiKeyHeaderName { get; set; } = "X-API-KEY";
    }

    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly IConfiguration _configuration;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfiguration configuration)
            : base(options, logger, encoder)
        {
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(Options.ApiKeyHeaderName, out var apiKeyHeaderValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var configuredApiKey = _configuration.GetSection("ApiKey").Value;
            if (string.IsNullOrWhiteSpace(configuredApiKey))
            {
                Logger.LogWarning("API Key is not configured in appsettings");
                return Task.FromResult(AuthenticateResult.Fail("API Key is not configured"));
            }

            if (!string.Equals(providedApiKey, configuredApiKey, StringComparison.Ordinal))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "env-mon-api-key"),
                new Claim(ClaimTypes.Name, "API Key User"),
                new Claim(ClaimTypes.Email, "env-mon"),
                new Claim(ClaimTypes.Role, GlobalRoles.Admin.ToString())
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
