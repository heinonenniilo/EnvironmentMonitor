using Azure.Messaging.EventHubs;

namespace EnvironmentMonitor.HubObserver.Extensions
{
    public static class EventDataExtensions
    {
        /// <summary>
        /// Gets the enqueued time from IoT Hub system properties.
        /// In v6.x, SystemProperties is a dictionary with key "iothub-enqueuedtime".
        /// </summary>
        public static DateTime? GetEnqueuedTimeUtc(this EventData eventData)
        {
            if (eventData.SystemProperties.TryGetValue("iothub-enqueuedtime", out var enqueuedTimeObj)
                && enqueuedTimeObj is DateTime enqueuedTime)
            {
                return enqueuedTime;
            }

            return null;
        }

        /// <summary>
        /// Gets the sequence number from Event Hub system properties.
        /// In v6.x, SystemProperties is a dictionary with key "x-opt-sequence-number".
        /// </summary>
        public static long? GetSequenceNumber(this EventData eventData)
        {
            if (eventData.SystemProperties.TryGetValue("x-opt-sequence-number", out var seqNumObj) 
                && seqNumObj is long seqNum)
            {
                return seqNum;
            }

            return null;
        }
    }
}
