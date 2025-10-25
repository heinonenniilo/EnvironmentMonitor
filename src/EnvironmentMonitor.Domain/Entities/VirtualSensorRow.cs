using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class VirtualSensorRow
    {
        public int Id { get; set; }
        public int VirtualSensorId { get; set; }
        public int ValueSensorId { get; set; }
        public int? TypeId { get; set; }
        public MeasurementType? Type { get; set; }
        public Sensor VirtualSensor { get; set; }
        public Sensor ValueSensor { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
