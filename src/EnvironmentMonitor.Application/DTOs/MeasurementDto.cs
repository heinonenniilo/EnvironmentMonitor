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
        public Dictionary<MeasurementTypes, MeasurementDto> MinValues { get; set; } = new Dictionary<MeasurementTypes, MeasurementDto>();
        public Dictionary<MeasurementTypes, MeasurementDto> MaxValues { get; set; } = new Dictionary<MeasurementTypes, MeasurementDto>();
    }

    public class MeasurementsViewModel
    {
        public List<MeasurementsBySensorDto> Measurements { get; set; } = new List<MeasurementsBySensorDto> { };
    }
}
