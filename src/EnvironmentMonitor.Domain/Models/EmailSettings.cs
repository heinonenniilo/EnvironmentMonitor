using EnvironmentMonitor.Domain.Enums;
using System;
using System.Collections.Generic;

namespace EnvironmentMonitor.Domain.Models
{
    public class AzureEmailSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
    }

    public class SmtpEmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;    
    }

    public class MailGunEmailSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        // MailGun requires a domain (like "mg.example.com")
        public string Domain { get; set; } = string.Empty;
        // Optional API base (default: https://api.mailgun.net)
        public string ApiBaseUrl { get; set; } = "https://api.mailgun.net";
    }

    public class EmailSettings
    {
        // Common fields across all email providers
        public string SenderAddress { get; set; } = string.Empty;
        public List<string> RecipientAddresses { get; set; } = new List<string>();
        public string EmailTitlePrefix { get; set; } = string.Empty;

        // Selected client type
        public EmailClientTypes ClientType { get; set; } = EmailClientTypes.AzureCommunicationService;

        // Provider-specific settings
        public AzureEmailSettings Azure { get; set; } = new AzureEmailSettings();
        public SmtpEmailSettings Smtp { get; set; } = new SmtpEmailSettings();
        public MailGunEmailSettings MailGun { get; set; } = new MailGunEmailSettings();
    }
}
