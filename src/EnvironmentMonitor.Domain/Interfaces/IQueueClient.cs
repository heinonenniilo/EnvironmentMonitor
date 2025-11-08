using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IQueueClient
    {
        Task SendMessage(string message);
        Task SendMessage(string queueName, string message);
    }
}
