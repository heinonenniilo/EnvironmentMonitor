using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid Identifier { get; set; }
        public IList<Device> Devices { get; set; }
        public IList<LocationSensor> LocationSensors { get; set; }
    }
}
