using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class MeasurementDto
    {
        public string DeviceId { get; set; }
        public List<MeasurementRow> Measurements { get; set; } = new List<MeasurementRow>();
    }

    public class MeasurementRow
    {
        public int SensorId { get; set; }
        public double SensorValue { get; set; }
        public int TypeId { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
