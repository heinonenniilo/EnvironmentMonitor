using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models;

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
        private readonly ApiKeySettings _apiKeySettings;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfiguration configuration,
            ApiKeySettings apiKeySettings)
            : base(options, logger, encoder)
        {
            _configuration = configuration;
            _apiKeySettings = apiKeySettings;
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

            if (_apiKeySettings.ApiKeys == null || _apiKeySettings.ApiKeys.Count == 0)
            {
                Logger.LogWarning("API Keys are not configured in appsettings");
                return Task.FromResult(AuthenticateResult.Fail("API Keys are not configured"));
            }

            var matchingKey = _apiKeySettings.ApiKeys.FirstOrDefault(k => 
                string.Equals(k.Key, providedApiKey, StringComparison.Ordinal));

            if (matchingKey == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "env-mon-api-key"),
                new(ClaimTypes.Name, "API Key User"),
                new(ClaimTypes.Email, "env-mon"),
                new(ClaimTypes.Role, GlobalRoles.User.ToString()),
                new(ClaimTypes.Role, GlobalRoles.ApiKeyUser.ToString())
            };

            if (!string.IsNullOrWhiteSpace(matchingKey.Devices))
            {
                var devices = matchingKey.Devices.Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => d.Trim())
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .ToList();

                if (devices.Any(d => d.Equals("ALL", StringComparison.OrdinalIgnoreCase)))
                {
                    claims.Add(new Claim(ClaimTypes.Role, GlobalRoles.MeasurementWriter.ToString()));
                }
                else
                {
                    foreach (var device in devices)
                    {
                        claims.Add(new Claim(EntityRoles.DeviceWriter.ToString(), device));
                    }
                }
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
