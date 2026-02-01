using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Entities;
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
        public Device Device { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
    }
}
