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
        public static int PublicMeasurementMaxLimitInDays = 7;
        public static int MeasurementMaxLimitInDays = 120;
        public static int DeviceWarningLimitInMinutes = 10;
        public static int DeviceLastMessageFetchLimitIndays = 1;

        public static string QueuedMessageDefaultKey = "VALUE";
        public static string QueuedMessageTimesStampKey = "{TIMESTAMP}";
        public static string QueuedMessageTimesStampPreviousKey = "{TIMESTAMP_PREVIOUS}";
        public static string QueuedMessageDeviceLink = "{DEVICE_LINK}";
        public static string QueuedMessageLocation = "{LOCATION}";
    }
}
