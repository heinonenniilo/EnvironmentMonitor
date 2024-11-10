using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class Device
    {
        public int Id { get; set; }
        public required string DeviceId { get; set; }
        public string Name { get; set; }
        public ICollection<Sensor> Sensors { get; set; }
    }
}
