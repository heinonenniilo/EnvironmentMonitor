namespace EnvironmentMonitor.Domain.Models
{
    public sealed class MqttSettings
    {
        public const string DefaultConnectionKey = "mqtt-default";
        public const string AdminConnectionKey = "mqtt-admin";
        public const string DynamicSecurityControlTopic = "$CONTROL/dynamic-security/v1";
        public const string DynamicSecurityResponseTopic = "$CONTROL/dynamic-security/v1/response";

        public string Host { get; set; } = "";
        public int Port { get; set; } = 8884;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string ClientId { get; set; } = "";
        public bool UseTls { get; set; } = true;
        public bool UseWebSockets { get; set; } = false;
        public string BaseTopic { get; set; } = "environment";

        public string AdminUsername { get; set; } = "";
        public string AdminPassword { get; set; } = "";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
        public bool IsAdminConfigured => IsConfigured && !string.IsNullOrWhiteSpace(AdminUsername) && !string.IsNullOrEmpty(AdminPassword);

        public string GetDeviceRoleName(string deviceIdentifier) => $"device-{deviceIdentifier}";

        /// <summary>
        /// Creates settings for the admin connection, using admin credentials and a separate client id.
        /// </summary>
        public MqttSettings CreateAdminSettings() => new()
        {
            Host = Host,
            Port = Port,
            Username = AdminUsername,
            Password = AdminPassword,
            ClientId = $"{ClientId}-admin",
            UseTls = UseTls,
            UseWebSockets = UseWebSockets,
            BaseTopic = BaseTopic,
        };

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
