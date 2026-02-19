namespace EnvironmentMonitor.Application.DTOs
{
    public class AddOrUpdatePublicSensorDto
    {
        public Guid? Identifier { get; set; }
        public required string Name { get; set; }
        public required Guid SensorIdentifier { get; set; }
        public int? TypeId { get; set; }
        public bool Active { get; set; } = true;
    }

    public class ManagePublicSensorsRequest
    {
        public List<AddOrUpdatePublicSensorDto> AddOrUpdate { get; set; } = [];
        public List<Guid> Remove { get; set; } = [];
    }
}
