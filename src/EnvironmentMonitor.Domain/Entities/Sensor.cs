using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class Sensor
    {
        public int Id { get; set; }
        public Guid Identifier { get; set; }
        public int DeviceId { get; set; }
        public Device Device { get; set; }
        public int SensorId { get; set; }
        public string Name { get; set; }
        public SensorType? Type { get; set; }
        public int? TypeId { get; set; }
        public ICollection<Measurement> Measurements { get; set; } = [];
        public ICollection<LocationSensor> LocationSensors { get; set; } = [];
        public ICollection<PublicSensor> PublicSensors { get; set; } = [];
        public ICollection<VirtualSensorRow> VirtualSensorRows { get; set; } = [];
        public ICollection<VirtualSensorRow> VirtualSensorRowValues { get; set; } = [];
        public double? ScaleMin { get; set; }
        public double? ScaleMax { get; set; }
        public bool IsVirtual { get; set; }
        public int? AggregationType { get; set; }
    }
}
