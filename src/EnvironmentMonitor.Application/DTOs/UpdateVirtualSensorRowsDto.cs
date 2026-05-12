namespace EnvironmentMonitor.Application.DTOs
{
    public class UpdateVirtualSensorRowsDto
    {
        public Guid DeviceIdentifier { get; set; }
        public Guid SensorIdentifier { get; set; }
        public List<AddVirtualSensorRowDto> RowsToAdd { get; set; } = [];
        public List<Guid> RowsToDelete { get; set; } = [];
    }

    public class AddVirtualSensorRowDto
    {
        public Guid ValueSensorIdentifier { get; set; }
        public int? TypeId { get; set; }
    }
}
