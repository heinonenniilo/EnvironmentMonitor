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
        public string BaseTopic { get; set; } = "environment";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);

        private string TopicRoot => BaseTopic.TrimEnd('/');
        public string MeasurementsTopicRoot => $"{TopicRoot}/measurements";
        public string CommandsTopicRoot => $"{TopicRoot}/commands";
        public string MeasurementsSubscriptionTopic => $"{MeasurementsTopicRoot}/+";
        public string GetMeasurementsTopic(string deviceIdentifier) => $"{MeasurementsTopicRoot}/{deviceIdentifier}";
        public string GetCommandsTopic(string deviceIdentifier) => $"{CommandsTopicRoot}/{deviceIdentifier}";

        /// <summary>
        /// Parses device identifier from a measurement topic ({BaseTopic}/measurements/{deviceIdentifier}).
        /// </summary>
        public string? GetDeviceIdentifierFromMeasurementTopic(string topic)
        {
            var prefix = $"{MeasurementsTopicRoot}/";
            if (!topic.StartsWith(prefix, StringComparison.Ordinal))
            {
                return null;
            }
            var identifier = topic.Substring(prefix.Length);
            return string.IsNullOrWhiteSpace(identifier) || identifier.Contains('/') ? null : identifier;
        }
    }
}
