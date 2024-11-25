using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IMeasurementService
    {
        public Task AddMeasurements(SaveMeasurementsDto measurent);
        public Task<List<MeasurementDto>> GetMeasurements(GetMeasurementsModel model);
        public Task<MeasurementsViewModel> GetMeasurementsBySensor(GetMeasurementsModel model);
        public Task<List<DeviceDto>> GetDevices();
        public Task<List<SensorDto>> GetSensors(List<string> DeviceIdentifier);
    }
}
