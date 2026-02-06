using System;

namespace EnvironmentMonitor.Application.DTOs
{
    public class AddOrUpdateSensorDto
    {
        public Guid? Identifier { get; set; }
        public Guid DeviceIdentifier { get; set; }
        public int? SensorId { get; set; }
        public required string Name { get; set; }
        public double? ScaleMin { get; set; }
        public double? ScaleMax { get; set; }
        public bool Active { get; set; } = true;
    }
}
