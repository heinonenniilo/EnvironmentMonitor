using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Enums
{
    public enum DeviceEventTypes
    {
        [Description("Reboot command")]
        RebootCommand = 0,
        [Description("First message after boot / online since")]
        Online = 1
    }
}
