using Azure;
using Azure.Communication.Email;
using Azure.Identity;
using AzureEmailClient = Azure.Communication.Email.EmailClient;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class AzureCommunicationServiceClient : IEmailClient
    {
        private readonly AzureEmailClient? _emailClient;
        private readonly EmailSettings _settings;
        private readonly ILogger<AzureCommunicationServiceClient> _logger;

        public AzureCommunicationServiceClient(EmailSettings settings, ILogger<AzureCommunicationServiceClient> logger)
        {
            _settings = settings;
            _logger = logger;

            var azure = _settings.Azure ?? new AzureEmailSettings();

            if (!string.IsNullOrEmpty(azure.Endpoint))
            {
                try
                {
                    var endpoint = new Uri(azure.Endpoint);
                    var credential = new DefaultAzureCredential();
                    _emailClient = new AzureEmailClient(endpoint, credential);
                    _logger.LogInformation("Azure Email client initialized with DefaultAzureCredential and endpoint: {Endpoint}", azure.Endpoint);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize Azure email client with DefaultAzureCredential");
                    throw;
                }
            }
            else if (!string.IsNullOrEmpty(azure.ConnectionString))
            {
                _emailClient = new AzureEmailClient(azure.ConnectionString);
                _logger.LogInformation("Azure Email client initialized with connection string");
            }
            else
            {
                _logger.LogWarning("Azure Email client not initialized - no connection string or endpoint provided");
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

            EmailUtils.ReplaceTokens(options);

            var toAddresses = EmailUtils.GetValidatedAndDistinctAddresses(options.ToAddresses);
            var ccAddresses = EmailUtils.GetValidatedAndDistinctAddresses(options.CcAddresses);
            var bccAddresses = EmailUtils.GetValidatedAndDistinctAddresses(_settings.RecipientAddresses, options.BccAddresses);

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

            foreach (var ccAddr in ccAddresses)
            {
                recipients.CC.Add(new EmailAddress(ccAddr));
            }

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
    }
}
