using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Enums
{
    public enum DeviceAttributeTypes
    {
        [Description("Output mode")]
        OutputMode = 0,
        [Description("Output delay in ms when motion control is on.")]
        OnDelay = 1,
    }
}
