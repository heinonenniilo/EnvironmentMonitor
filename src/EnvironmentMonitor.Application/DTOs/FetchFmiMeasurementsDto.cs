using System;
using System.Collections.Generic;

namespace EnvironmentMonitor.Application.DTOs
{
    public class FetchFmiMeasurementsDto
    {
        public List<string> Places { get; set; } = new();
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
    }
}
