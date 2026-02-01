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
        /// <summary>
        /// Fetch and store measurements from FMI.
        /// </summary>
        /// <returns></returns>
        Task<int> FetchAndStoreMeasurements(FetchFmiMeasurementsRequest request);
        /// <summary>
        /// Fetch FMI data for marked device based on CommunicationChannelId. Calls FetchAndStoreMeasurements for each device.
        /// </summary>
        /// <returns></returns>
        Task SyncData();
    }

    public class FetchFmiMeasurementsRequest
    {
        public Device Device { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
    }
}
