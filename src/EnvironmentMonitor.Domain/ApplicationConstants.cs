using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain
{
    public static class ApplicationConstants
    {
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
        public static string EmailTemplateMfaCodeKey = "{MfaCode}";
        // Authentication claim types
        public static string ExternalLoginProviderClaim = "ExternalLoginProvider";

        // Multi factor authentication
        // Used as the "login provider" key for the pending UPN entry stored in AspNetUserTokens.
        public static string MfaTokenProviderName = "[EnvironmentMonitorMfa]";
        public static string MfaPendingUpnTokenName = "PendingUpn";
        public static string MfaIssuer = "EnvironmentMonitor";
        public static int MfaCodeValidityInMinutes = 5;
        // Token purpose used for confirming the email address of a signed in user with a code.
        public static string EmailConfirmationCodePurpose = "EmailConfirmationCode";

        // API Key authentication header names
        public static string ApiKeyHeaderName = "X-API-KEY";
        public static string SecretIdHeaderName = "X-SECRET-ID";
        public static string SecretValueHeaderName = "X-SECRET-VALUE";

        // Sync service status variable keys
        public static string SyncLastDeviceMessageIdKey = "Sync_LastDeviceMessageId";

        // Hangfire job parameter used to mark jobs which have a matching queued command row to acknowledge
        public static string HangfireQueuedCommandParameter = "QueuedCommand";

        public static int FirstMessageLimitInMinutes = 5;
    }
}
