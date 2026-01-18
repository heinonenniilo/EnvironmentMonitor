using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;

namespace EnvironmentMonitor.Tests.Mocks
{
    /// <summary>
    /// Mock queue client for testing that doesn't actually send messages to Azure Queue
    /// </summary>
    public class MockQueueClient : IQueueClient
    {
        public Task<QueueMessageInfo> SendMessage(string message, TimeSpan? delay = null)
        {
            return Task.FromResult(new QueueMessageInfo
            {
                MessageId = Guid.NewGuid().ToString(),
                PopReceipt = Guid.NewGuid().ToString(),
                ScheludedToExecuteUtc = DateTime.UtcNow.Add(delay ?? TimeSpan.Zero),
                MessageText = message,
                InsertedOnUtc = DateTime.UtcNow
            });
        }

        public Task<QueueMessageInfo> SendMessage(string queueName, string message, TimeSpan? delay = null)
        {
            return SendMessage(message, delay);
        }

        public Task DeleteMessage(string messageId, string popReceipt)
        {
            return Task.CompletedTask;
        }

        public Task DeleteMessage(string queueName, string messageId, string popReceipt)
        {
            return Task.CompletedTask;
        }

        public Task<QueueMessageInfo> UpdateMessageVisibility(string messageId, string popReceipt, TimeSpan visibilityTimeout)
        {
            return Task.FromResult(new QueueMessageInfo
            {
                MessageId = messageId,
                PopReceipt = Guid.NewGuid().ToString(),
                ScheludedToExecuteUtc = DateTime.UtcNow.Add(visibilityTimeout),
                MessageText = string.Empty,
                InsertedOnUtc = DateTime.UtcNow
            });
        }

        public Task<QueueMessageInfo> UpdateMessageVisibility(string queueName, string messageId, string popReceipt, TimeSpan visibilityTimeout)
        {
            return UpdateMessageVisibility(messageId, popReceipt, visibilityTimeout);
        }
    }
}
