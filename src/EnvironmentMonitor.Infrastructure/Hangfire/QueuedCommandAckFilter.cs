using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Interfaces;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace EnvironmentMonitor.Infrastructure.Hangfire
{
    /// <summary>
    /// Acknowledges queued device commands after the corresponding Hangfire job has been executed.
    /// Only jobs tagged with the <see cref="ApplicationConstants.HangfireQueuedCommandParameter"/> job parameter are handled.
    /// The Hangfire job id is stored as the message id of the queued command.
    /// </summary>
    public class QueuedCommandAckFilter : IServerFilter
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<QueuedCommandAckFilter> _logger;

        public QueuedCommandAckFilter(IServiceScopeFactory scopeFactory, ILogger<QueuedCommandAckFilter> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public void OnPerforming(PerformingContext context)
        {
        }

        public void OnPerformed(PerformedContext context)
        {
            if (!IsQueuedCommand(context))
            {
                return;
            }

            var jobId = context.BackgroundJob.Id;
            var hasFailed = context.Exception != null && !context.ExceptionHandled;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var ackService = scope.ServiceProvider.GetRequiredService<IQueuedCommandAckService>();
                var dateService = scope.ServiceProvider.GetRequiredService<IDateService>();

                // Null date indicates a failure
                var executedAt = hasFailed ? (DateTime?)null : dateService.CurrentTime();
                ackService.AckQueuedCommand(jobId, executedAt).GetAwaiter().GetResult();

                _logger.LogInformation("Acknowledged queued command for job: {JobId}. ExecutedAt: {ExecutedAt}", jobId, executedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to acknowledge queued command for job: {JobId}", jobId);
            }
        }

        private bool IsQueuedCommand(PerformedContext context)
        {
            try
            {
                return string.Equals(
                    context.GetJobParameter<string>(ApplicationConstants.HangfireQueuedCommandParameter),
                    bool.TrueString,
                    StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read job parameter for job: {JobId}", context.BackgroundJob.Id);
                return false;
            }
        }
    }
}
