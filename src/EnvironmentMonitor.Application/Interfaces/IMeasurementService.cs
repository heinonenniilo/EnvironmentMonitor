using EnvironmentMonitor.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IMeasurementService
    {
        public Task AddMeasurements(MeasurementDto measurent);
    }
}
