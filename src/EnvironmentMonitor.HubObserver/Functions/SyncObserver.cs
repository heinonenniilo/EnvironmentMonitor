using EnvironmentMonitor.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EnvironmentMonitor.HubObserver.Functions
{
    /// <summary>
    /// Scheduled function to sync device messages to another EnvironmentMonitor instance
    /// </summary>
    public class SyncObserver
    {
        private readonly ILogger<SyncObserver> _logger;
        private readonly ISyncService _syncService;

        public SyncObserver(ILogger<SyncObserver> logger, ISyncService syncService)
        {
            _logger = logger;
            _syncService = syncService;
        }

        [Function(nameof(SyncObserver))]
        [FixedDelayRetry(3, "00:00:05")]
        public async Task Run([TimerTrigger("%SyncSchedule%")] TimerInfo timerInfo,
            FunctionContext context)
        {
            _logger.LogInformation("Starting sync operation");

            try
            {
                var syncedCount = await _syncService.SyncMessages();

                if (syncedCount > 0)
                {
                    _logger.LogInformation($"Successfully synced {syncedCount} device messages");
                }
                else
                {
                    _logger.LogInformation("No messages to sync or sync is disabled");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sync operation");
                throw; // Re-throw to trigger retry policy
            }
        }
    }
}
