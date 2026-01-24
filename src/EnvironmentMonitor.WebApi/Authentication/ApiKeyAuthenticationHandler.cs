using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using EnvironmentMonitor.Application.Interfaces;
using System.Collections.Generic;

namespace EnvironmentMonitor.WebApi.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ApiKey";
        public string Scheme => DefaultScheme;
        public string ApiKeyHeaderName { get; set; } = "X-API-KEY";
        public string SecretIdHeaderName { get; set; } = "X-SECRET-ID";
        public string SecretValueHeaderName { get; set; } = "X-SECRET-VALUE";
    }

    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly ApiKeySettings _apiKeySettings;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IApiKeyService apiKeyService,
            ApiKeySettings apiKeySettings)
            : base(options, logger, encoder)
        {
            _apiKeyService = apiKeyService;
            _apiKeySettings = apiKeySettings;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Step 1: Validate API key from configuration
            if (!Request.Headers.TryGetValue(Options.ApiKeyHeaderName, out var apiKeyHeaderValues))
            {
                return AuthenticateResult.NoResult();
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return AuthenticateResult.NoResult();
            }

            if (_apiKeySettings.ApiKeys == null || _apiKeySettings.ApiKeys.Count == 0)
            {
                Logger.LogWarning("No API keys configured in appsettings");
                return AuthenticateResult.Fail("API key authentication not configured");
            }
            // Check if the provided API key matches any of the configured keys
            var matchingApiKey = _apiKeySettings.ApiKeys.FirstOrDefault(k =>
                string.Equals(k, providedApiKey, System.StringComparison.Ordinal));
            if (matchingApiKey == null)
            {
                Logger.LogWarning("Invalid API key provided");
                return AuthenticateResult.Fail("Invalid API Key");
            }
            // Step 2: Validate secret ID and secret value
            if (!Request.Headers.TryGetValue(Options.SecretIdHeaderName, out var secretIdHeaderValues))
            {
                Logger.LogWarning("Secret ID header missing");
                return AuthenticateResult.Fail("Secret ID required");
            }

            if (!Request.Headers.TryGetValue(Options.SecretValueHeaderName, out var secretValueHeaderValues))
            {
                Logger.LogWarning("Secret value header missing");
                return AuthenticateResult.Fail("Secret value required");
            }

            var providedSecretId = secretIdHeaderValues.FirstOrDefault();
            var providedSecretValue = secretValueHeaderValues.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(providedSecretId) || string.IsNullOrWhiteSpace(providedSecretValue))
            {
                Logger.LogWarning("Secret ID or secret value is empty");
                return AuthenticateResult.Fail("Invalid secret credentials");
            }
            // Step 3: Verify the secret and get it with claims if valid
            var secret = await _apiKeyService.VerifyApiKey(providedSecretId, providedSecretValue);           
            if (secret == null)
            {
                Logger.LogWarning($"Secret value verification failed for secret ID '{providedSecretId}'");
                return AuthenticateResult.Fail("Invalid secret value");
            }
            // Step 4: Build claims from the validated secret
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, $"api-secret-{secret.Id}"),
                new(ClaimTypes.Name, $"API Secret User ({secret.Description ?? secret.Id})"),
                new(ClaimTypes.Email, "api-secret"),
                new(ClaimTypes.Role, GlobalRoles.ApiKeyUser.ToString())
            };
            // Add claims from the secret
            foreach (var secretClaim in secret.Claims)
            {
                if (secretClaim.Type == EntityRoles.Device.ToString())
                {
                    claims.Add(new Claim(EntityRoles.DeviceWriter.ToString(), secretClaim.Value));
                }
                else if (secretClaim.Type == EntityRoles.Location.ToString())
                {
                    claims.Add(new Claim(EntityRoles.Location.ToString(), secretClaim.Value));
                }
            }
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            Logger.LogInformation($"Successfully authenticated with secret ID '{secret.Id}'");
            return AuthenticateResult.Success(ticket);
        }
    }
}
