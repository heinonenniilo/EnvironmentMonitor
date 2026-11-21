namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IMqttMessageService
    {
        public Task SendMessageToDevice(string deviceIdentifier, string message, CancellationToken cancellationToken = default);
    }
}
