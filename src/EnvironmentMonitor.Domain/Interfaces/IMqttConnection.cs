namespace EnvironmentMonitor.Domain.Interfaces
{
    /// <summary>
    /// Generic MQTT connection. Knows only about topics and payloads, not about their meaning.
    /// </summary>
    public interface IMqttConnection
    {
        Task Publish(string topic, string payload, CancellationToken cancellationToken = default);

        /// <summary>
        /// Subscribes to <paramref name="topicFilter"/> (wildcards supported). Subscriptions are restored automatically after reconnects.
        /// <paramref name="handler"/> is invoked with (topic, payload). Dispose the returned handle to unsubscribe.
        /// </summary>
        Task<IAsyncDisposable> Subscribe(string topicFilter, Func<string, string, Task> handler, CancellationToken cancellationToken = default);
    }
}