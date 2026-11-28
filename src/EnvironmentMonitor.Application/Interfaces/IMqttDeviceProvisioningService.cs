namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IMqttDeviceProvisioningService
    {
        /// <summary>
        /// Creates the MQTT client, its device role and topic permissions.
        /// </summary>
        Task RegisterDevice(Guid identifier, string password, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the MQTT client and its device role.
        /// </summary>
        Task UnregisterDevice(Guid identifier, CancellationToken cancellationToken = default);
    }
}
