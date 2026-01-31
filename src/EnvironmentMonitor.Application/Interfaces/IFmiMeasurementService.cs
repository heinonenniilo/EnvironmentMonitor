using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IFmiMeasurementService
    {
        Task<int> FetchAndStoreMeasurementsAsync(FetchFmiMeasurementsRequest request);
        Task PerformSync();
    }

    public class FetchFmiMeasurementsRequest
    {
        public List<string> Places { get; set; } = new();
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
    }
}
