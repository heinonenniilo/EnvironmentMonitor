using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class SensorType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public ICollection<Sensor> Sensors { get; set; }
    }
}
