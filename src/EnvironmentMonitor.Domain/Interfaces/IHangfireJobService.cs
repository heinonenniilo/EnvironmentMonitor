using System.Linq.Expressions;

namespace EnvironmentMonitor.Domain.Interfaces
{
    /// <summary>
    /// Service for enqueuing background jobs with Hangfire.
    /// Provides a clean abstraction over Hangfire job execution.
    /// </summary>
    public interface IHangfireJobService
    {
        /// <summary>
        /// Enqueues a fire-and-forget job to be executed immediately in the background.
        /// </summary>
        /// <typeparam name="TService">The service type containing the method to execute</typeparam>
        /// <param name="methodCall">Expression pointing to the method to execute</param>
        /// <returns>Job ID</returns>
        string Enqueue<TService>(Expression<Func<TService, Task>> methodCall);

        /// <summary>
        /// Enqueues a delayed job to be executed after a specified delay.
        /// </summary>
        /// <typeparam name="TService">The service type containing the method to execute</typeparam>
        /// <param name="methodCall">Expression pointing to the method to execute</param>
        /// <param name="delay">Time to wait before executing the job</param>
        /// <param name="jobParameters">Optional job parameters stored with the job</param>
        /// <returns>Job ID</returns>
        string Schedule<TService>(Expression<Func<TService, Task>> methodCall, TimeSpan delay, IDictionary<string, string>? jobParameters = null);

        /// <summary>
        /// Enqueues a delayed job to be executed at a specific time.
        /// </summary>
        /// <typeparam name="TService">The service type containing the method to execute</typeparam>
        /// <param name="methodCall">Expression pointing to the method to execute</param>
        /// <param name="enqueueAt">DateTime when the job should be executed</param>
        /// <param name="jobParameters">Optional job parameters stored with the job</param>
        /// <returns>Job ID</returns>
        string Schedule<TService>(Expression<Func<TService, Task>> methodCall, DateTimeOffset enqueueAt, IDictionary<string, string>? jobParameters = null);

        /// <summary>
        /// Checks if Hangfire is available and configured.
        /// </summary>
        bool IsAvailable { get; }
    }
}
