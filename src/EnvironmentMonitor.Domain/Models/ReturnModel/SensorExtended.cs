using EnvironmentMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models.ReturnModel
{
    public class SensorExtended : Sensor
    {
        public Guid DeviceIdentifier { get; set; }
    }
}
