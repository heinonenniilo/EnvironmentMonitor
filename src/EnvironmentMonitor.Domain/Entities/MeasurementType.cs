using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class MeasurementType
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Unit { get; set; }
        public List<Measurement> Measurements { get; set; }
    }
}
