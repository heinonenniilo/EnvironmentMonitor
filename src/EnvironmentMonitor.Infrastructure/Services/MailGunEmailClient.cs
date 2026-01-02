using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Utils;
using Microsoft.Extensions.Logging;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class MailGunEmailClient : IEmailClient
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<MailGunEmailClient> _logger;
        private readonly HttpClient _httpClient;

        public MailGunEmailClient(EmailSettings settings, ILogger<MailGunEmailClient> logger, IHttpClientFactory httpClientFactory)
        {
            _settings = settings;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("MailGun");
        }

        public async Task SendEmailAsync(SendEmailOptions options)
        {
            var mg = _settings.MailGun ?? new MailGunEmailSettings();
            if (string.IsNullOrWhiteSpace(_settings.SenderAddress))
                throw new InvalidOperationException("Sender address not configured");
            if (string.IsNullOrWhiteSpace(mg.ApiKey) || string.IsNullOrWhiteSpace(mg.Domain))
                throw new InvalidOperationException("MailGun ApiKey or Domain not configured");

            EmailUtils.ReplaceTokens(options);

            var toAddresses = EmailUtils.GetValidatedAndDistinctAddresses(options.ToAddresses);
            var ccAddresses = EmailUtils.GetValidatedAndDistinctAddresses(options.CcAddresses);
            var bccAddresses = EmailUtils.GetValidatedAndDistinctAddresses(_settings.RecipientAddresses, options.BccAddresses);

            if (!toAddresses.Any() && !ccAddresses.Any() && !bccAddresses.Any())
            {
                _logger.LogWarning("No recipient addresses configured or provided. Email not sent.");
                return;
            }

            var subjectToUse = string.IsNullOrEmpty(_settings.EmailTitlePrefix)
                ? options.Subject
                : $"{_settings.EmailTitlePrefix} {options.Subject}";

            var apiBase = string.IsNullOrWhiteSpace(mg.ApiBaseUrl) ? "https://api.mailgun.net" : mg.ApiBaseUrl.TrimEnd('/');
            var url = $"{apiBase}/v3/{mg.Domain}/messages";

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{mg.ApiKey}"));
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            var form = new List<KeyValuePair<string, string>>
            {
                new("from", _settings.SenderAddress),
                new("subject", subjectToUse)
            };
            foreach (var addr in toAddresses) form.Add(new("to", addr));
            foreach (var addr in ccAddresses) form.Add(new("cc", addr));
            foreach (var addr in bccAddresses) form.Add(new("bcc", addr));

            if (!string.IsNullOrWhiteSpace(options.HtmlContent))
                form.Add(new("html", options.HtmlContent));
            if (!string.IsNullOrWhiteSpace(options.PlainTextContent))
                form.Add(new("text", options.PlainTextContent));

            req.Content = new FormUrlEncodedContent(form);

            try
            {
                var resp = await _httpClient.SendAsync(req);
                resp.EnsureSuccessStatusCode();
                _logger.LogInformation("MailGun email sent successfully to {Count} recipients", toAddresses.Count + ccAddresses.Count + bccAddresses.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send MailGun email");
                throw;
            }
        }
    }
}
