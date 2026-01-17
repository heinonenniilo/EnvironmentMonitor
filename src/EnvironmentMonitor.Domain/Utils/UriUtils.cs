using System;

namespace EnvironmentMonitor.Domain.Utils
{
    public static class UriUtils
    {
        /// <summary>
        /// Validates that a string is a valid absolute HTTP or HTTPS URL.
        /// </summary>
        /// <param name="url">The URL string to validate.</param>
        /// <returns>True if the URL is valid and uses HTTP or HTTPS scheme; otherwise, false.</returns>
        public static bool IsValidHttpUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Validates that a string is a valid absolute URL with any scheme.
        /// </summary>
        /// <param name="url">The URL string to validate.</param>
        /// <returns>True if the URL is a valid absolute URL; otherwise, false.</returns>
        public static bool IsValidAbsoluteUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        /// <summary>
        /// Tries to create a valid Uri from a string.
        /// </summary>
        /// <param name="url">The URL string to parse.</param>
        /// <param name="uri">When this method returns, contains the Uri equivalent of the url if the conversion succeeded, or null if it failed.</param>
        /// <returns>True if url was converted successfully; otherwise, false.</returns>
        public static bool TryParseUri(string? url, out Uri? uri)
        {
            uri = null;
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            return Uri.TryCreate(url, UriKind.Absolute, out uri);
        }

        /// <summary>
        /// Combines multiple URL parts into a single URL, properly handling slashes.
        /// </summary>
        /// <param name="baseUrl">The base URL (e.g., "https://example.com" or "https://example.com/").</param>
        /// <param name="parts">Additional URL parts to append (e.g., "api", "users", "123").</param>
        /// <returns>The combined URL string.</returns>
        /// <exception cref="ArgumentException">Thrown when baseUrl is null, empty, or not a valid absolute URL.</exception>
        public static string CombineUrl(string baseUrl, params string[] parts)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentException("Base URL cannot be null or empty.", nameof(baseUrl));
            }

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                throw new ArgumentException("Base URL must be a valid absolute URL.", nameof(baseUrl));
            }

            if (parts == null || parts.Length == 0)
            {
                return baseUrl.TrimEnd('/');
            }

            var result = baseUrl.TrimEnd('/');

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                var trimmedPart = part.Trim().Trim('/');
                if (!string.IsNullOrEmpty(trimmedPart))
                {
                    result = $"{result}/{trimmedPart}";
                }
            }

            return result;
        }
    }
}
