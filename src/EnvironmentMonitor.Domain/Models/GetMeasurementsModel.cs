using EnvironmentMonitor.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class GetMeasurementsModel
    {
        public List<Guid> SensorIdentifiers { get; set; } = [];
        /// <summary>
        /// If no sensor ids provided, and this filter is provided, it will filter by sensors and for each sensor, on measurement types in the Dictionary.
        /// </summary>
        public Dictionary<int, List<MeasurementTypes>?> ? SensorsByTypeFilter { get; set; }
        public List<long>? DeviceMessageIds { get; set; }
        public DateTime From { get; set; } = DateTime.UtcNow.AddDays(-1);
        public DateTime? To { get; set; }
        public bool? LatestOnly { get; set; }
    }
}
