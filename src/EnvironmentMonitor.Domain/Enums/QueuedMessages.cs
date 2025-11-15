using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Enums
{
    public enum QueuedMessages
    {
        SendDeviceAttributes = 0,
        SetMotionControlStatus = 1,
        SetMotionControlOnDelay = 2
    }
}
