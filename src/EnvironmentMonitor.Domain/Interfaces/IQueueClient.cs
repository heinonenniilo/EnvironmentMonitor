using EnvironmentMonitor.Domain.Models;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IQueueClient
    {
        Task<CreateQueuedMessageReturnModel> SendMessage(string message, TimeSpan? delay= null);
        Task<CreateQueuedMessageReturnModel> SendMessage(string queueName, string message, TimeSpan? delay= null);
        Task<IEnumerable<QueueMessageInfo>> PeekMessages(string queueName, int maxMessages = 32);
        Task DeleteMessage(string messageId, string popReceipt);
        Task DeleteMessage(string queueName, string messageId, string popReceipt);
    }

    public class QueueMessageInfo
    {
        public string MessageId { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public string? PopReceipt { get; set; }
        public DateTimeOffset? InsertedOn { get; set; }
        public DateTimeOffset? ExpiresOn { get; set; }
        public DateTimeOffset? NextVisibleOn { get; set; }
        public long DequeueCount { get; set; }
    }
}
