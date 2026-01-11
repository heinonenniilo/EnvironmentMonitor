using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EnvironmentMonitor.Application.Services
{
    public class UserCookieService : IUserCookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationSettings _applicationSettings;
        private const string AuthInfoCookieName = "AuthInfoCookie";

        public UserCookieService(IHttpContextAccessor httpContextAccessor, ApplicationSettings applicationSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _applicationSettings = applicationSettings;
        }

        public void WriteAuthFailureCookie(List<string> errors, string? errorCode = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available");
            }

            var authInfo = new AuthInfoCookie
            {
                LoginState = false,
                Errors = errors,
                ErrorCode = errorCode,
                Timestamp = DateTime.UtcNow
            };

            WriteAuthCookie(httpContext, authInfo);
        }

        public void WriteAuthSuccessCookie()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available");
            }

            var authInfo = new AuthInfoCookie
            {
                LoginState = true,
                Errors = new List<string>(),
                ErrorCode = null,
                Timestamp = DateTime.UtcNow
            };

            WriteAuthCookie(httpContext, authInfo);
        }

        public AuthInfoCookie? ReadAuthInfoCookie()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available");
            }

            if (!httpContext.Request.Cookies.TryGetValue(AuthInfoCookieName, out var cookieValue))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<AuthInfoCookie>(cookieValue);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public void ClearAuthInfoCookie()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available");
            }

            httpContext.Response.Cookies.Delete(AuthInfoCookieName);
        }

        private void WriteAuthCookie(HttpContext httpContext, AuthInfoCookie authInfo)
        {
            var serializedData = JsonSerializer.Serialize(authInfo);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = _applicationSettings.IsProduction ? SameSiteMode.Strict : SameSiteMode.None,
                MaxAge = TimeSpan.FromHours(1),
            };

            httpContext.Response.Cookies.Append(AuthInfoCookieName, serializedData, cookieOptions);
        }
    }
}
