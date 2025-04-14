using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class LocationSensor
    {
        public required Location Location { get; set; }
        public int LocationId { get; set; }
        public required Sensor Sensor { get; set; }
        public int SensorId { get; set; }
        public required Device Device { get; set; }
        public int DeviceId { get; set; }
        public required string Name { get; set; }
        public int? TypeId { get; set; }
        public MeasurementType? MeasurementType { get; set; }
    }
}
