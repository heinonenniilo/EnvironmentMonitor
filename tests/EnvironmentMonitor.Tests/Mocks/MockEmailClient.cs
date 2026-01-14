using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;

namespace EnvironmentMonitor.Tests.Mocks
{
    /// <summary>
    /// Mock email client for testing that doesn't actually send emails
    /// </summary>
    public class MockEmailClient : IEmailClient
    {
        public List<SendEmailOptions> SentEmails { get; } = new();

        public Task SendEmailAsync(SendEmailOptions options)
        {
            // Store the email for verification in tests if needed
            SentEmails.Add(options);
            
            // Don't actually send any emails
            return Task.CompletedTask;
        }

        public void ClearSentEmails()
        {
            SentEmails.Clear();
        }
    }
}
