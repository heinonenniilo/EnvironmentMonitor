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
    }
}
