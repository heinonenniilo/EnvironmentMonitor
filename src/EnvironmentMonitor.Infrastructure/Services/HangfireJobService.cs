using EnvironmentMonitor.Domain.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace EnvironmentMonitor.Infrastructure.Services
{
    /// <summary>
    /// Service for enqueuing background jobs with Hangfire.
    /// Provides a clean abstraction that falls back gracefully if Hangfire is not configured.
    /// </summary>
    public class HangfireJobService : IHangfireJobService
    {
        private readonly IBackgroundJobClient? _backgroundJobClient;
        private readonly ILogger<HangfireJobService> _logger;

        public HangfireJobService(
            ILogger<HangfireJobService> logger,
            IBackgroundJobClient? backgroundJobClient = null)
        {
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
        }

        /// <summary>
        /// Checks if Hangfire is available and configured.
        /// </summary>
        public bool IsAvailable => _backgroundJobClient != null;

        /// <summary>
        /// Enqueues a fire-and-forget job to be executed immediately in the background.
        /// </summary>
        /// <typeparam name="TService">The service type containing the method to execute</typeparam>
        /// <param name="methodCall">Expression pointing to the method to execute</param>
        /// <returns>Job ID, or empty string if Hangfire is not available</returns>
        /// <example>
        /// jobService.Enqueue&lt;IUserAuthService&gt;(service => service.ForgotPassword(model));
        /// </example>
        public string Enqueue<TService>(Expression<Func<TService, Task>> methodCall)
        {
            if (_backgroundJobClient == null)
            {
                _logger.LogError("Hangfire is not configured. Job will not be enqueued: {MethodCall}", methodCall);
                throw new InvalidOperationException("Hangfire is not configured. Cannot enqueue job.");
            }

            try
            {
                var jobId = _backgroundJobClient.Enqueue(methodCall);
                _logger.LogInformation("Job enqueued successfully. Job ID: {JobId}", jobId);
                return jobId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue Hangfire job");
                throw;
            }
        }

        /// <summary>
        /// Enqueues a delayed job to be executed after a specified delay.
        /// </summary>
        /// <typeparam name="TService">The service type containing the method to execute</typeparam>
        /// <param name="methodCall">Expression pointing to the method to execute</param>
        /// <param name="delay">Time to wait before executing the job</param>
        /// <returns>Job ID, or empty string if Hangfire is not available</returns>
        /// <example>
        /// jobService.Schedule&lt;IEmailService&gt;(service => service.SendEmail(email), TimeSpan.FromMinutes(5));
        /// </example>
        public string Schedule<TService>(Expression<Func<TService, Task>> methodCall, TimeSpan delay)
        {
            if (_backgroundJobClient == null)
            {
                _logger.LogWarning("Hangfire is not configured. Scheduled job will not be enqueued: {MethodCall}", methodCall);
                return string.Empty;
            }

            try
            {
                var jobId = _backgroundJobClient.Schedule(methodCall, delay);
                _logger.LogInformation("Job scheduled successfully for {Delay}. Job ID: {JobId}", delay, jobId);
                return jobId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule Hangfire job");
                throw;
            }
        }

        /// <summary>
        /// Enqueues a delayed job to be executed at a specific time.
        /// </summary>
        /// <typeparam name="TService">The service type containing the method to execute</typeparam>
        /// <param name="methodCall">Expression pointing to the method to execute</param>
        /// <param name="enqueueAt">DateTime when the job should be executed</param>
        /// <returns>Job ID, or empty string if Hangfire is not available</returns>
        /// <example>
        /// jobService.Schedule&lt;IReportService&gt;(service => service.GenerateReport(), DateTimeOffset.UtcNow.AddHours(2));
        /// </example>
        public string Schedule<TService>(Expression<Func<TService, Task>> methodCall, DateTimeOffset enqueueAt)
        {
            if (_backgroundJobClient == null)
            {
                _logger.LogWarning("Hangfire is not configured. Scheduled job will not be enqueued: {MethodCall}", methodCall);
                return string.Empty;
            }

            try
            {
                var jobId = _backgroundJobClient.Schedule(methodCall, enqueueAt);
                _logger.LogInformation("Job scheduled successfully for {EnqueueAt}. Job ID: {JobId}", enqueueAt, jobId);
                return jobId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule Hangfire job");
                throw;
            }
        }
    }
}
