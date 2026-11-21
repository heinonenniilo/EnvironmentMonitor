namespace EnvironmentMonitor.Domain.Interfaces
{
    /// <summary>
    /// Handles measurement messages received through MQTT.
    /// </summary>
    public interface IMqttMeasurementHandler
    {
        public Task HandleMeasurementMessage(string deviceIdentifier, string payload);
    }
}
