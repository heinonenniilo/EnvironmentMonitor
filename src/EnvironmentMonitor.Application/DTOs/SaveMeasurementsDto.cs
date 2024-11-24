using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class SaveMeasurementsDto
    {
        public string DeviceId { get; set; }
        public List<MeasurementDto> Measurements { get; set; } = new List<MeasurementDto>();
    }

    public class MeasurementDto
    {
        public int SensorId { get; set; }
        public double SensorValue { get; set; }
        public int TypeId { get; set; }
        public DateTime TimestampUtc { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
