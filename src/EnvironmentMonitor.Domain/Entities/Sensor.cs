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
        public int DeviceId { get; set; }
        public Device Device { get; set; }
        public int SensorId { get; set; }

        public string Name { get; set; }
        public SensorType Type { get; set; }
        public int TypeId { get; set; } 

        public ICollection<Measurement> Measurements { get; set; }
    }    
}
