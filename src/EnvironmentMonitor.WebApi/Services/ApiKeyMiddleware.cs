using EnvironmentMonitor.WebApi.Attributes;

namespace EnvironmentMonitor.WebApi.Services
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeaderName = "X-API-KEY";
        private IConfiguration _configuration;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null && endpoint.Metadata.GetMetadata<ApiKeyRequiredAttribute>() != null)
            {
                if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("API Key is missing.");
                    return;
                }

                var configuredApiKey = _configuration.GetSection("ApiKey").Value;
                if (!string.Equals(extractedApiKey, configuredApiKey, StringComparison.Ordinal))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid API Key.");
                    return;
                }
            }
            await _next(context);
        }
    }
}
