namespace EnvironmentMonitor.Application.DTOs
{
    public class MoveDevicesToLocationDto
    {
        public Guid LocationIdentifier { get; set; }
        public List<Guid> DeviceIdentifiers { get; set; } = [];
    }
}
