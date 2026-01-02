using System;
using System.Collections.Generic;
using System.Linq;
using EnvironmentMonitor.Domain.Models;

namespace EnvironmentMonitor.Domain.Utils
{
    public static class EmailUtils
    {
        public static void ReplaceTokens(SendEmailOptions options)
        {
            if (options.ReplaceTokens == null || !options.ReplaceTokens.Any())
            {
                return;
            }

            foreach (var token in options.ReplaceTokens)
            {
                if (!string.IsNullOrEmpty(options.Subject))
                {
                    options.Subject = options.Subject.Replace(token.Key, token.Value);
                }
                if (!string.IsNullOrEmpty(options.HtmlContent))
                {
                    options.HtmlContent = options.HtmlContent.Replace(token.Key, token.Value);
                }
                if (!string.IsNullOrEmpty(options.PlainTextContent))
                {
                    options.PlainTextContent = options.PlainTextContent.Replace(token.Key, token.Value);
                }
            }
        }

        public static List<string> GetValidatedAndDistinctAddresses(params object?[] addressSources)
        {
            var addresses = new List<string>();
            foreach (var source in addressSources)
            {
                if (source == null) 
                { 
                    continue; 
                }
                if (source is string singleAddress)
                {
                    if (!string.IsNullOrWhiteSpace(singleAddress))
                    {
                        addresses.Add(singleAddress);
                    }
                }
                else if (source is IEnumerable<string> addressList)
                {
                    addresses.AddRange(addressList.Where(addr => !string.IsNullOrWhiteSpace(addr)));
                }
            }
            return addresses.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
