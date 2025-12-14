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

            if (!string.IsNullOrEmpty(_settings.ConnectionString))
            {
                _emailClient = new AzureEmailClient(_settings.ConnectionString);
                _logger.LogInformation("Email client initialized with connection string");
            }
            else if (!string.IsNullOrEmpty(_settings.Endpoint))
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
            else
            {
                _logger.LogWarning("Email client not initialized - no connection string or endpoint provided");
            }
        }

        public async Task SendEmailAsync(string subject, string htmlContent, string plainTextContent = "")
        {
            if (_emailClient == null)
            {
                throw new InvalidOperationException("Email client not initialized");
            }

            if (string.IsNullOrEmpty(_settings.SenderAddress))
            {
                throw new InvalidOperationException("Sender address not configured");
            }

            if (_settings.RecipientAddresses == null || !_settings.RecipientAddresses.Any())
            {
                _logger.LogWarning("No recipient addresses configured. Email not sent.");
                return;
            }

            _logger.LogInformation($"Preparing to send email to {_settings.RecipientAddresses.Count} recipient(s). Subject: {subject}");

            var subjectToUse = string.IsNullOrEmpty(_settings.EmailTitlePrefix)
                ? subject
                : $"{_settings.EmailTitlePrefix} {subject}";
            var emailContent = new EmailContent(subjectToUse)
            {
                Html = htmlContent
            };
            if (!string.IsNullOrEmpty(plainTextContent))
            {
                emailContent.PlainText = plainTextContent;
            }

            var recipients = new EmailRecipients(_settings.RecipientAddresses.Select(addr => new EmailAddress(addr)).ToList());
            var emailMessage = new EmailMessage(_settings.SenderAddress, recipients, emailContent);

            try
            {
                EmailSendOperation emailSendOperation = await _emailClient.SendAsync(WaitUntil.Completed, emailMessage);
                _logger.LogInformation($"Email sent successfully. Message ID: {emailSendOperation.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email. Subject: {subject}");
                throw;
            }
        }
    }
}
