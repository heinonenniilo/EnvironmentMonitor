using EnvironmentMonitor.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class MeasurementDto
    {
        public int SensorId { get; set; }
        public double SensorValue { get; set; }
        public int TypeId { get; set; }
        public DateTime TimestampUtc { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class MeasurementsBySensorDto
    {
        public int SensorId { get; set; }
        public List<MeasurementDto> Measurements { get; set; } = new List<MeasurementDto>();
        public Dictionary<int, MeasurementDto> MinValues { get; set; } = new Dictionary<int, MeasurementDto>();
        public Dictionary<int, MeasurementDto> MaxValues { get; set; } = new Dictionary<int, MeasurementDto>();
        public Dictionary<int, MeasurementDto> LatestValues { get; set; } = new Dictionary<int, MeasurementDto>();
    }

    public class MeasurementsViewModel
    {
        public List<MeasurementsBySensorDto> Measurements { get; set; } = new List<MeasurementsBySensorDto> { };
    }
}
