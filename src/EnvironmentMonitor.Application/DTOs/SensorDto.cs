using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class SensorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SensorId { get; set; }
        public int DeviceId { get; set; }
        public double? ScaleMin { get; set; }
        public double? ScaleMax { get; set; }
    }
}
