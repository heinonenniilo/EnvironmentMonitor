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
        public Task<MeasurementsModel> GetMeasurements(GetMeasurementsModel model);
        public Task<MeasurementsBySensorModel> GetMeasurementsBySensor(GetMeasurementsModel model);
    }
}
