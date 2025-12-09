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

        public async Task<QueueMessageInfo> SendMessage(string message, TimeSpan? delay)
        {
            if (string.IsNullOrEmpty(_settings.DefaultQueueName))
            {
                throw new InvalidOperationException("Default queue name not configured");
            }

            return await SendMessage(_settings.DefaultQueueName, message, delay);
        }

        public async Task<QueueMessageInfo> SendMessage(string queueName, string message, TimeSpan? delay = null)
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

                var scheduledTimeUtc = res.Value.TimeNextVisible.UtcDateTime;
                var insertedOnUtc = res.Value.InsertionTime.UtcDateTime;

                return new QueueMessageInfo
                {
                    MessageId = res.Value.MessageId,
                    PopReceipt = res.Value.PopReceipt,
                    ScheludedToExecuteUtc = scheduledTimeUtc,
                    MessageText = message,
                    InsertedOnUtc = insertedOnUtc
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send message to queue '{queueName}': {message}");
                throw;
            }
        }

        public async Task DeleteMessage(string queueName, string messageId, string popReceipt)
        {
            if (_queueServiceClient == null)
            {
                throw new InvalidOperationException("Queue client not initialized");
            }

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);

                _logger.LogInformation($"Deleting message from queue '{queueName}'. MessageId: {messageId}");

                await queueClient.DeleteMessageAsync(messageId, popReceipt);

                _logger.LogInformation($"Successfully deleted message from queue '{queueName}'. MessageId: {messageId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete message from queue '{queueName}'. MessageId: {messageId}");
                throw;
            }
        }

        public async Task DeleteMessage(string messageId, string popReceipt)
        {
            if (string.IsNullOrEmpty(_settings.DefaultQueueName))
            {
                throw new InvalidOperationException("Default queue name not configured");
            }

            await DeleteMessage(_settings.DefaultQueueName, messageId, popReceipt);
        }

        public async Task<QueueMessageInfo> UpdateMessageVisibility(string messageId, string popReceipt, TimeSpan visibilityTimeout)
        {
            if (string.IsNullOrEmpty(_settings.DefaultQueueName))
            {
                throw new InvalidOperationException("Default queue name not configured");
            }

            return await UpdateMessageVisibility(_settings.DefaultQueueName, messageId, popReceipt, visibilityTimeout);
        }

        public async Task<QueueMessageInfo> UpdateMessageVisibility(string queueName, string messageId, string popReceipt, TimeSpan visibilityTimeout)
        {
            if (_queueServiceClient == null)
            {
                throw new InvalidOperationException("Queue client not initialized");
            }

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);

                _logger.LogInformation($"Updating message visibility in queue '{queueName}'. MessageId: {messageId}, VisibilityTimeout: {visibilityTimeout}");

                var res = await queueClient.UpdateMessageAsync(messageId, popReceipt, visibilityTimeout: visibilityTimeout);

                _logger.LogInformation($"Successfully updated message visibility in queue '{queueName}'. MessageId: {messageId}");

                var scheduledTimeUtc = res.Value.NextVisibleOn.UtcDateTime;

                return new QueueMessageInfo
                {
                    MessageId = messageId,
                    PopReceipt = res.Value.PopReceipt,
                    ScheludedToExecuteUtc = scheduledTimeUtc,
                    MessageText = string.Empty,
                    InsertedOnUtc = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update message visibility in queue '{queueName}'. MessageId: {messageId}");
                throw;
            }
        }
    }
}
