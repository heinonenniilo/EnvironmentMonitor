using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class Measurement
    {
        public long Id { get; set; }
        public required int SensorId { get; set; }
        public Sensor Sensor { get; set; }
        public required int TypeId { get; set; }
        public MeasurementType Type { get; set; }
        public double Value { get; set; }
        public required DateTime Timestamp { get; set; }
        public required DateTime TimestampUtc { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DeviceMessage? DeviceMessage { get; set; }
        public int? DeviceMessageId { get; set; }
    }
}
