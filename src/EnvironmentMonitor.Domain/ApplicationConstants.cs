using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain
{
    public static class ApplicationConstants
    {
        public static string TargetTimeZone = "FLE Standard Time";

        public static int MeasurementGroupByLimitInDays = 7;
        public static int DeviceWarningLimitInMinutes = 10;
        public static int DeviceLastMessageFetchLimitIndays = 1;
    }
}
