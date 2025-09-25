using EnvironmentMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models.ReturnModel
{
    public class MeasurementExtended : Measurement
    {
        public Guid SensorGuid { get; set; }
    }
}
