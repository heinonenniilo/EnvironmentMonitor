using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Enums
{
    public enum DeviceEmailTemplateTypes
    {
        [Description("Device '{DeviceName}' / '{DeviceIdentifier}' has lost connection. Last seen: {LastSeen}.")]
        ConnectionLost = 0,
        [Description("Device '{DeviceName}' / '{DeviceIdentifier}' connection has been restored. Online since: {OnlineSince}.")]
        ConnectionOk = 1
    }
}
