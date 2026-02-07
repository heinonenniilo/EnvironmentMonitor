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
        public static int PublicMeasurementMaxLimitInDays = 5;
        public static int PublicMeasurementMaxLimitInDaysForRegistered = 14;
        public static int PublicMeasurementMaxLimitInDaysForUsers = 60;
        public static int MeasurementMaxLimitInDays = 120;
        public static int DeviceWarningLimitInMinutes = 10;
        public static int VirtualDeviceWarningLimitInMinutes = 20;
        public static int DeviceLastMessageFetchLimitIndays = 1;

        public static string QueuedMessageDefaultKey = "VALUE";
        public static string QueuedMessageApplicationBaseUrlKey = "{BASE_URL}";
        public static string QueuedMessageTimesStampKey = "{TIMESTAMP}";
        public static string QueuedMessageTimesStampPreviousKey = "{TIMESTAMP_PREVIOUS}";
        public static string QueuedMessageDeviceLink = "{DEVICE_LINK}";
        public static string QueuedMessageLocation = "{LOCATION}";
        public static string EmailTemplateConfirmationLinkKey = "{ConfirmationLink}";
        public static string EmailTemplatePasswordResetLinkKey = "{ResetLink}";
        // Authentication claim types
        public static string ExternalLoginProviderClaim = "ExternalLoginProvider";
    }
}
