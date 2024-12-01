using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class GetMeasurementsModel
    {
        public List<int> SensorIds { get; set; } = new List<int>();
        public DateTime From { get; set; }
        public DateTime? To { get; set; }
        public bool? LatestOnly { get; set; }
    }
}
