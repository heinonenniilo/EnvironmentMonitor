using System;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    /// <summary>
    /// Acknowledges queued device commands by their message id.
    /// Used by background job infrastructure, which only knows the job / message id.
    /// </summary>
    public interface IQueuedCommandAckService
    {
        /// <summary>
        /// Acknowledges a queued command. Null date indicates a failure.
        /// </summary>
        /// <param name="messageId">Message id of the queued command. For Hangfire jobs this is the job id.</param>
        /// <param name="date">When completed. If NULL, will be interpreted as failure to complete.</param>
        Task AckQueuedCommand(string messageId, DateTime? date);
    }
}
