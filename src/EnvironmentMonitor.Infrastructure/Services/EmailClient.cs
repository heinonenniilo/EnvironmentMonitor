using Azure;
using Azure.Communication.Email;
using Azure.Identity;
using AzureEmailClient = Azure.Communication.Email.EmailClient;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class EmailClient : IEmailClient
    {
        private readonly AzureEmailClient? _emailClient;
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailClient> _logger;     

        public EmailClient(EmailSettings settings, ILogger<EmailClient> logger)
        {
            _settings = settings;
            _logger = logger;

            if (!string.IsNullOrEmpty(_settings.Endpoint))
            {
                try
                {
                    var endpoint = new Uri(_settings.Endpoint);
                    var credential = new DefaultAzureCredential();
                    _emailClient = new AzureEmailClient(endpoint, credential);
                    _logger.LogInformation("Email client initialized with DefaultAzureCredential and endpoint: {Endpoint}", _settings.Endpoint);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize email client with DefaultAzureCredential");
                    throw;
                }
            }
            else if (!string.IsNullOrEmpty(_settings.ConnectionString))
            {
                _emailClient = new AzureEmailClient(_settings.ConnectionString);
                _logger.LogInformation("Email client initialized with connection string");
            }
            else
            {
                _logger.LogWarning("Email client not initialized - no connection string or endpoint provided");
            }
        }

        public async Task SendEmailAsync(SendEmailOptions options)
        {
            if (_emailClient == null)
            {
                throw new InvalidOperationException("Email client not initialized");
            }
            if (string.IsNullOrEmpty(_settings.SenderAddress))
            {
                throw new InvalidOperationException("Sender address not configured");
            }

            var toAddresses = GetValidatedAndDistinctAddresses(options.ToAddresses);
            var ccAddresses = GetValidatedAndDistinctAddresses(options.CcAddresses);
            var bccAddresses = GetValidatedAndDistinctAddresses(_settings.RecipientAddresses, options.BccAddresses);

            // Validate that we have at least one recipient
            if (!toAddresses.Any() && !ccAddresses.Any() && !bccAddresses.Any())
            {
                _logger.LogWarning("No recipient addresses configured or provided. Email not sent.");
                return;
            }

            _logger.LogInformation($"Preparing to send email. To: {toAddresses.Count}, CC: {ccAddresses.Count}, BCC: {bccAddresses.Count}. Subject: {options.Subject}");

            var subjectToUse = string.IsNullOrEmpty(_settings.EmailTitlePrefix)
                ? options.Subject
                : $"{_settings.EmailTitlePrefix} {options.Subject}";

            var emailContent = new EmailContent(subjectToUse)
            {
                Html = options.HtmlContent
            };

            if (!string.IsNullOrEmpty(options.PlainTextContent))
            {
                emailContent.PlainText = options.PlainTextContent;
            }

            var recipients = new EmailRecipients(toAddresses.Select(addr => new EmailAddress(addr)).ToList());

            // Add CC recipients
            foreach (var ccAddr in ccAddresses)
            {
                recipients.CC.Add(new EmailAddress(ccAddr));
            }

            // Add BCC recipients
            foreach (var bccAddr in bccAddresses)
            {
                recipients.BCC.Add(new EmailAddress(bccAddr));
            }

            var emailMessage = new EmailMessage(_settings.SenderAddress, recipients, emailContent);

            try
            {
                EmailSendOperation emailSendOperation = await _emailClient.SendAsync(WaitUntil.Completed, emailMessage);
                _logger.LogInformation($"Email sent successfully. Message ID: {emailSendOperation.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email. Subject: {options.Subject}");
                throw;
            }
        }

        private static List<string> GetValidatedAndDistinctAddresses(params object?[] addressSources)
        {
            var addresses = new List<string>();
            
            foreach (var source in addressSources)
            {
                if (source == null)
                    continue;

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
