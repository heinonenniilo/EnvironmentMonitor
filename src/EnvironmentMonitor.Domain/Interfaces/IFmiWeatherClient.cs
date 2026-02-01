using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IFmiWeatherClient : IDisposable
    {
        /// <summary>
        /// Retrieves weather measurement series from FMI for multiple places.
        /// </summary>
        /// <param name="places">List of place names (e.g., "Helsinki", "Tampere")</param>
        /// <param name="startTimeUtc">Start time in UTC</param>
        /// <param name="endTimeUtc">End time in UTC</param>
        /// <param name="parameters">FMI parameter codes (e.g., "t2m" for temperature, "rh" for humidity)</param>
        /// <returns>
        /// Dictionary where:
        /// - Key: Place name (e.g., "Helsinki Kaisaniemi")
        /// - Value: Dictionary where:
        ///   - Key: Parameter code (e.g., "t2m", "rh")
        ///   - Value: List of data points with timestamps and values
        /// </returns>
        Task<Dictionary<string, Dictionary<string, List<FmiDataPoint>>>> GetSeriesAsync(
            IEnumerable<string> places,
            DateTime startTimeUtc,
            DateTime endTimeUtc,
            IEnumerable<string> parameters);
    }
}
