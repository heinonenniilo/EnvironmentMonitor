using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IFmiWeatherClient : IDisposable
    {
        Task<Dictionary<string, List<(DateTime Time, double Value)>>> GetSeriesAsync(
            string place,
            DateTime startTimeUtc,
            DateTime endTimeUtc,
            IEnumerable<string> parameters);
    }
}
