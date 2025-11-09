using Azure.Identity;
using Azure.Storage.Queues;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class QueueClient : IQueueClient
    {
        private readonly QueueServiceClient? _queueServiceClient;
        private readonly ILogger<QueueClient> _logger;
        private readonly QueueSettings _settings;

        public QueueClient(QueueSettings settings, ILogger<QueueClient> logger)
        {
            _logger = logger;
            _settings = settings;
            var clientOptions = new QueueClientOptions()
            {
                MessageEncoding = QueueMessageEncoding.Base64
            };
            // Try to initialize using QueueServiceUri first, then AccountName
            if (!string.IsNullOrEmpty(settings.QueueServiceUri))
            {
                var queueServiceUri = new Uri(settings.QueueServiceUri);
                _queueServiceClient = new QueueServiceClient(queueServiceUri, new DefaultAzureCredential(), clientOptions);
                _logger.LogInformation($"Queue client initialized using QueueServiceUri: {settings.QueueServiceUri}");
            }
            else if (!string.IsNullOrEmpty(settings.AccountName))
            {
                var queueServiceUri = new Uri($"https://{settings.AccountName}.queue.core.windows.net");
                _queueServiceClient = new QueueServiceClient(queueServiceUri, new DefaultAzureCredential(), clientOptions);
                _logger.LogInformation($"Queue client initialized using AccountName: {settings.AccountName}");
            }
            else
            {
                _logger.LogWarning("Queue Service URI or Account Name not configured");
            }
        }

        public async Task SendMessage(string message, TimeSpan? delay)
        {
            if (string.IsNullOrEmpty(_settings.DefaultQueueName))
            {
                throw new InvalidOperationException("Default queue name not configured");
            }

            await SendMessage(_settings.DefaultQueueName, message, delay);
        }

        public async Task SendMessage(string queueName, string message, TimeSpan? delay)
        {
            if (_queueServiceClient == null)
            {
                throw new InvalidOperationException("Queue client not initialized");
            }

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);

                await queueClient.CreateIfNotExistsAsync();

                _logger.LogInformation($"Sending message to queue '{queueName}': {message}");

                await queueClient.SendMessageAsync(message, visibilityTimeout: delay);

                _logger.LogInformation($"Successfully sent message to queue '{queueName}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send message to queue '{queueName}': {message}");
                throw;
            }
        }
    }
}
