using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class MeasurementSource
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public IList<DeviceMessage> DeviceMessages { get; set; }
    }
}
