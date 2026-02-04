using EnvironmentMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Utils
{
    public static class DeviceUtils
    {
        public static int GetOfflineThresholdInMinutes(this Device device)
        {
            if (device.IsVirtual)
            {
                return ApplicationConstants.VirtualDeviceWarningLimitInMinutes;
            }
            return ApplicationConstants.DeviceWarningLimitInMinutes;
        }
    }
}
