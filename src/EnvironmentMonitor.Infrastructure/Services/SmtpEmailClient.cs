using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Utils;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class SmtpEmailClient : IEmailClient
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailClient> _logger;

        public SmtpEmailClient(EmailSettings settings, ILogger<SmtpEmailClient> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task SendEmailAsync(SendEmailOptions options)
        {
            var smtp = _settings.Smtp ?? new SmtpEmailSettings();

            if (string.IsNullOrWhiteSpace(_settings.SenderAddress))
                throw new InvalidOperationException("Sender address not configured");
            if (string.IsNullOrWhiteSpace(smtp.Host))
                throw new InvalidOperationException("SMTP host not configured");

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

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_settings.SenderAddress));
            message.To.AddRange(toAddresses.Select(addr => MailboxAddress.Parse(addr)));
            message.Cc.AddRange(ccAddresses.Select(addr => MailboxAddress.Parse(addr)));
            message.Bcc.AddRange(bccAddresses.Select(addr => MailboxAddress.Parse(addr)));
            message.Subject = subjectToUse;

            var bodyBuilder = new BodyBuilder();
            if (!string.IsNullOrWhiteSpace(options.HtmlContent))
            {
                bodyBuilder.HtmlBody = options.HtmlContent;
            }
            if (!string.IsNullOrWhiteSpace(options.PlainTextContent))
            {
                bodyBuilder.TextBody = options.PlainTextContent;
            }
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                var secureOption = smtp.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
                await client.ConnectAsync(smtp.Host, smtp.Port, secureOption);

                if (!string.IsNullOrEmpty(smtp.Username))
                {
                    await client.AuthenticateAsync(smtp.Username, smtp.Password);
                }

                await client.SendAsync(message);
                _logger.LogInformation("SMTP email sent successfully to {Count} recipients", toAddresses.Count + ccAddresses.Count + bccAddresses.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMTP email");
                throw;
            }
            finally
            {
                try
                {
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to disconnect from SMTP host");
                }
            }
        }
    }
}
