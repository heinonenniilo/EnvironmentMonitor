using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;

namespace EnvironmentMonitor.Infrastructure.Services
{
    /// <summary>
    /// Owns the MQTT client connection. Used for publishing commands to devices and for subscribing to device topics.
    /// </summary>
    public sealed class MqttClientService : IMqttMessageService, IAsyncDisposable
    {
        private readonly MqttSettings _settings;
        private readonly ILogger<MqttClientService> _logger;
        private readonly MqttClientFactory _factory = new();
        private readonly SemaphoreSlim _connectLock = new(1, 1);
        private readonly MqttClientOptions _options;

        public IMqttClient Client { get; }
        public MqttSettings Settings => _settings;

        public MqttClientService(MqttSettings settings, ILogger<MqttClientService> logger)
        {
            _settings = settings;
            _logger = logger;
            Client = _factory.CreateMqttClient();
            _options = BuildOptions(settings);
        }

        public async Task EnsureConnectedAsync(CancellationToken cancellationToken)
        {
            if (Client.IsConnected)
            {
                return;
            }

            await _connectLock.WaitAsync(cancellationToken);
            try
            {
                if (Client.IsConnected)
                {
                    return;
                }
                _logger.LogInformation($"Connecting to MQTT broker '{_settings.Host}:{_settings.Port}'");
                await Client.ConnectAsync(_options, cancellationToken);
                _logger.LogInformation("Connected to MQTT broker");
            }
            finally
            {
                _connectLock.Release();
            }
        }

        public async Task SendMessageToDevice(string deviceIdentifier, string message, CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);
            var topic = _settings.GetCommandsTopic(deviceIdentifier);
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            _logger.LogInformation($"Publishing MQTT message to topic '{topic}'");
            var result = await Client.PublishAsync(applicationMessage, cancellationToken);
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException($"Failed to publish MQTT message to topic '{topic}'. Reason: {result.ReasonCode} {result.ReasonString}");
            }
        }

        public MqttClientSubscribeOptions BuildSubscribeOptions(string topic) =>
            _factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(topic).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                .Build();

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (Client.IsConnected)
                {
                    await Client.DisconnectAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to disconnect MQTT client gracefully");
            }
            Client.Dispose();
            _connectLock.Dispose();
        }

        private static MqttClientOptions BuildOptions(MqttSettings settings)
        {
            var builder = new MqttClientOptionsBuilder()
                .WithCleanSession(false)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(30));

            if (settings.UseWebSockets)
            {
                var scheme = settings.UseTls ? "wss" : "ws";
                builder.WithWebSocketServer(o => o.WithUri($"{scheme}://{settings.Host}:{settings.Port}/mqtt"));
            }
            else
            {
                builder.WithTcpServer(settings.Host, settings.Port);
            }

            builder.WithClientId(settings.ClientId);

            if (!string.IsNullOrEmpty(settings.Username))
            {
                builder.WithCredentials(settings.Username, settings.Password);
            }

            if (settings.UseTls)
            {
                builder.WithTlsOptions(o => o.UseTls());
            }

            return builder.Build();
        }
    }
}
