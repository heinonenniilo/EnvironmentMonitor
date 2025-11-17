using EnvironmentMonitor.Domain.Models;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IQueueClient
    {
        Task<QueueMessageInfo> SendMessage(string message, TimeSpan? delay= null);
        Task<QueueMessageInfo> SendMessage(string queueName, string message, TimeSpan? delay= null);
        Task DeleteMessage(string messageId, string popReceipt);
        Task DeleteMessage(string queueName, string messageId, string popReceipt);
    }
}
