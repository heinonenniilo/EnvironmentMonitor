using System;

namespace EnvironmentMonitor.Domain.Models
{
    /// <summary>
    /// Represents a single weather measurement data point from FMI (Finnish Meteorological Institute).
    /// </summary>
    public class FmiDataPoint
    {
        /// <summary>
        /// The timestamp of the measurement in UTC.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// The measured value (e.g., temperature in Celsius, humidity percentage).
        /// </summary>
        public double Value { get; set; }
    }
}
