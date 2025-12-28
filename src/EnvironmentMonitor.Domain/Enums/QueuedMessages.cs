using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Enums
{
    public enum QueuedMessages
    {
        [Description("Send stored device attributes")]
        SendDeviceAttributes = 0,
        [Description("Set motion control status")]
        SetMotionControlStatus = 1,
        [Description("Set motion control delay")]
        SetMotionControlOnDelay = 2,
        [Description("Send email about a device")]
        SendDeviceEmail = 3,
        [Description("Process forget user password request")]
        ProcessForgetUserPasswordRequest = 4
    }
}
