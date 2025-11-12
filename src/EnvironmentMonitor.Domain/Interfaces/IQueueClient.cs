using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IQueueClient
    {
        Task SendMessage(string message, TimeSpan? delay= null);
        Task SendMessage(string queueName, string message, TimeSpan? delay= null);
    }
}
