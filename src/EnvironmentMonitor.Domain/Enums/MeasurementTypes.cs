using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Enums
{
    public enum MeasurementTypes
    {
        Undefined = 0,
        Temperature = 1,
        Humidity = 2,
        Light = 3,
        Motion = 4,
        Pressure = 5
    }
}
