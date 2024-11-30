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

    public class MeasurementsBySensorModel
    {
        public List<MeasurementsBySensorDto> Measurements { get; set; } = new List<MeasurementsBySensorDto> { };
    }

    public class MeasurementsModel
    {
        public List<MeasurementDto> Measurements { get; set; } = [];
        public List<MeasurementsInfoDto> MeasurementsInfo { get; set; } = [];
    }

    public class MeasurementsInfoDto
    {
        public int SensorId { get; set; }
        public Dictionary<int, MeasurementDto> MinValues { get; set; } = [];
        public Dictionary<int, MeasurementDto> MaxValues { get; set; } = [];
        public Dictionary<int, MeasurementDto> LatestValues { get; set; } = [];
    }

    public class MeasurementsBySensorDto : MeasurementsInfoDto
    {
        public List<MeasurementDto> Measurements { get; set; } = [];
    }
}
