namespace EnvironmentMonitor.Domain.Models
{
    public sealed class MqttSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 8884;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string ClientId { get; set; } = "";
        public bool UseTls { get; set; } = true;
        public bool UseWebSockets { get; set; } = false;
        public string BaseTopic { get; set; } = "environmentmonitor";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);

        public string DevicesTopicRoot => $"{BaseTopic.TrimEnd('/')}/devices";
        public string MeasurementsSubscriptionTopic => $"{DevicesTopicRoot}/+/measurements";
        public string GetMeasurementsTopic(string deviceIdentifier) => $"{DevicesTopicRoot}/{deviceIdentifier}/measurements";
        public string GetCommandsTopic(string deviceIdentifier) => $"{DevicesTopicRoot}/{deviceIdentifier}/commands";

        /// <summary>
        /// Parses device identifier from a measurement topic ({BaseTopic}/devices/{deviceIdentifier}/measurements).
        /// </summary>
        public string? GetDeviceIdentifierFromMeasurementTopic(string topic)
        {
            var prefix = $"{DevicesTopicRoot}/";
            const string suffix = "/measurements";
            if (!topic.StartsWith(prefix, StringComparison.Ordinal) || !topic.EndsWith(suffix, StringComparison.Ordinal))
            {
                return null;
            }
            var identifier = topic.Substring(prefix.Length, topic.Length - prefix.Length - suffix.Length);
            return string.IsNullOrWhiteSpace(identifier) || identifier.Contains('/') ? null : identifier;
        }
    }
}
