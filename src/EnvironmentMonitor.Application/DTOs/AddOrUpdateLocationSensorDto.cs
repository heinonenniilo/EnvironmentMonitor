namespace EnvironmentMonitor.Application.DTOs
{
    public class AddOrUpdateLocationSensorDto
    {
        public Guid LocationIdentifier { get; set; }
        public Guid SensorIdentifier { get; set; }
        public required string Name { get; set; }
        public int? TypeId { get; set; }
    }
}
