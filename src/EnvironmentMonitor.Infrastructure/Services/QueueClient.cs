using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class QueueClient : IQueueClient
    {
        private readonly QueueServiceClient? _queueServiceClient;
        private readonly ILogger<QueueClient> _logger;
        private readonly QueueSettings _settings;
        private readonly IDateService _dateService;

        public QueueClient(QueueSettings settings, ILogger<QueueClient> logger, IDateService dateService)
        {
            _logger = logger;
            _settings = settings;
            _dateService = dateService;
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

        public async Task<CreateQueuedMessageReturnModel> SendMessage(string message, TimeSpan? delay)
        {
            if (string.IsNullOrEmpty(_settings.DefaultQueueName))
            {
                throw new InvalidOperationException("Default queue name not configured");
            }

            return await SendMessage(_settings.DefaultQueueName, message, delay);
        }

        public async Task<CreateQueuedMessageReturnModel> SendMessage(string queueName, string message, TimeSpan? delay = null)
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

                var res = await queueClient.SendMessageAsync(message, visibilityTimeout: delay);

                _logger.LogInformation($"Successfully sent message to queue '{queueName}'");
                return new CreateQueuedMessageReturnModel
                {
                    MessageId = res.Value.MessageId,
                    ScheludedToExecute = _dateService.CurrentTime().Add(delay ?? TimeSpan.Zero)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send message to queue '{queueName}': {message}");
                throw;
            }
        }

        public async Task<IEnumerable<QueueMessageInfo>> PeekMessages(string queueName, int maxMessages = 32)
        {
            if (_queueServiceClient == null)
            {
                throw new InvalidOperationException("Queue client not initialized");
            }

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);

                _logger.LogInformation($"Peeking messages from queue '{queueName}' (max: {maxMessages})");

                var response = await queueClient.PeekMessagesAsync(maxMessages);

                var messages = response.Value.Select(m => new QueueMessageInfo
                {
                    MessageId = m.MessageId,
                    MessageText = m.MessageText,
                    PopReceipt = null,
                    InsertedOn = m.InsertedOn,
                    ExpiresOn = m.ExpiresOn,
                    NextVisibleOn = null, // Not available with PeekMessages
                    DequeueCount = m.DequeueCount
                }).ToList();

                _logger.LogInformation($"Peeked {messages.Count} messages from queue '{queueName}'");

                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to peek messages from queue '{queueName}'");
                throw;
            }
        }
    }
}
