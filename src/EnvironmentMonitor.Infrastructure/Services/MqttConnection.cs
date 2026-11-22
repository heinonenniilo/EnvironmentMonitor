using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using System.Collections.Concurrent;

namespace EnvironmentMonitor.Infrastructure.Services
{
    /// <summary>
    /// Owns the MQTT client: connection, publishing, subscriptions and message dispatching.
    /// Contains no knowledge of application specific topics or payloads.
    /// </summary>
    public sealed class MqttConnection : IMqttConnection, IAsyncDisposable
    {
        private readonly MqttSettings _settings;
        private readonly ILogger<MqttConnection> _logger;
        private readonly MqttClientFactory _factory = new();
        private readonly SemaphoreSlim _connectLock = new(1, 1);
        private readonly MqttClientOptions _options;
        private readonly IMqttClient _client;
        private readonly ConcurrentDictionary<Guid, MqttSubscription> _subscriptions = new();

        public MqttConnection(MqttSettings settings, ILogger<MqttConnection> logger)
        {
            _settings = settings;
            _logger = logger;
            _options = BuildOptions(settings);
            _client = _factory.CreateMqttClient();
            _client.ConnectedAsync += OnConnectedAsync;
            _client.DisconnectedAsync += OnDisconnectedAsync;
            _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        }

        public async Task Publish(string topic, string payload, CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            _logger.LogInformation($"Publishing MQTT message to topic '{topic}'");
            var result = await _client.PublishAsync(message, cancellationToken);
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException($"Failed to publish MQTT message to topic '{topic}'. Reason: {result.ReasonCode} {result.ReasonString}");
            }
        }

        public async Task<IAsyncDisposable> Subscribe(string topicFilter, Func<string, string, Task> handler, CancellationToken cancellationToken = default)
        {
            var subscription = new MqttSubscription(Guid.NewGuid(), topicFilter, handler, this);
            _subscriptions[subscription.Id] = subscription;

            if (_client.IsConnected)
            {
                await SubscribeToBrokerAsync(topicFilter, cancellationToken);
            }
            return subscription;
        }

        public async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
        {
            if (_client.IsConnected)
            {
                return;
            }

            await _connectLock.WaitAsync(cancellationToken);
            try
            {
                if (_client.IsConnected)
                {
                    return;
                }
                _logger.LogInformation($"Connecting to MQTT broker '{_settings.Host}:{_settings.Port}'");
                await _client.ConnectAsync(_options, cancellationToken);
                _logger.LogInformation("Connected to MQTT broker");
            }
            finally
            {
                _connectLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_client.IsConnected)
                {
                    await _client.DisconnectAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to disconnect MQTT client gracefully");
            }
            _client.Dispose();
            _connectLock.Dispose();
        }
        private async Task Unsubscribe(MqttSubscription subscription)
        {
            if (!_subscriptions.TryRemove(subscription.Id, out _))
            {
                return;
            }

            var filterStillUsed = _subscriptions.Values.Any(s => s.TopicFilter == subscription.TopicFilter);
            if (!filterStillUsed && _client.IsConnected)
            {
                _logger.LogInformation($"Unsubscribing from MQTT topic '{subscription.TopicFilter}'");
                await _client.UnsubscribeAsync(subscription.TopicFilter);
            }
        }

        private async Task OnConnectedAsync(MqttClientConnectedEventArgs e)
        {
            foreach (var topicFilter in _subscriptions.Values.Select(s => s.TopicFilter).Distinct())
            {
                await SubscribeToBrokerAsync(topicFilter, CancellationToken.None);
            }
        }

        private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            if (e.ClientWasConnected)
            {
                _logger.LogWarning(e.Exception, $"Disconnected from MQTT broker. Reason: {e.Reason}");
            }
            return Task.CompletedTask;
        }

        private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = e.ApplicationMessage.ConvertPayloadToString();

            foreach (var subscription in _subscriptions.Values)
            {
                if (MqttTopicFilterComparer.Compare(topic, subscription.TopicFilter) != MqttTopicFilterCompareResult.IsMatch)
                {
                    continue;
                }

                try
                {
                    await subscription.Handler(topic, payload);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"MQTT handler for '{subscription.TopicFilter}' failed for topic '{topic}'");
                }
            }
        }

        private async Task SubscribeToBrokerAsync(string topicFilter, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Subscribing to MQTT topic '{topicFilter}'");
            var options = _factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(topicFilter).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                .Build();
            await _client.SubscribeAsync(options, cancellationToken);
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

        private sealed class MqttSubscription : IAsyncDisposable
        {
            private readonly MqttConnection _owner;

            public Guid Id { get; }
            public string TopicFilter { get; }
            public Func<string, string, Task> Handler { get; }

            public MqttSubscription(Guid id, string topicFilter, Func<string, string, Task> handler, MqttConnection owner)
            {
                Id = id;
                TopicFilter = topicFilter;
                Handler = handler;
                _owner = owner;
            }

            public async ValueTask DisposeAsync() => await _owner.Unsubscribe(this);
        }
    }
}
