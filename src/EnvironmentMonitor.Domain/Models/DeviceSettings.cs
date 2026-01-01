namespace EnvironmentMonitor.Domain.Models
{
    public class DeviceSettings
    {
        // If false, device emails will not be queued/sent
        public bool SendDeviceEmails { get; set; } = true;
    }
}
