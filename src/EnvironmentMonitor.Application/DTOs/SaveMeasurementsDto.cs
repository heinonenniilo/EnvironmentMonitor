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
        public List<AddMeasurementDto> Measurements { get; set; } = new List<AddMeasurementDto>();
        public bool FirstMessage { get; set; }
        public DateTime? EnqueuedUtc { get; set; }
        public long? SequenceNumber { get; set; }

        /// <summary>
        /// Uptime in ms (=millis())
        /// </summary>
        public long? Uptime { get; set; }
        public string? Identifier { get; set; }
        public long? LoopCount { get; set; }
        public int? MessageCount { get; set; }
    }
}
