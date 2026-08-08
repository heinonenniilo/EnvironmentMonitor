using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models.GetModels;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    /// <summary>
    /// Acknowledges queued device commands by their message id.
    /// Used from background job infrastructure, which only knows the job / message id.
    /// </summary>
    public class QueuedCommandAckService : IQueuedCommandAckService
    {
        private readonly ILogger<QueuedCommandAckService> _logger;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IDateService _dateService;

        public QueuedCommandAckService(
            ILogger<QueuedCommandAckService> logger,
            IDeviceRepository deviceRepository,
            IDateService dateService)
        {
            _logger = logger;
            _deviceRepository = deviceRepository;
            _dateService = dateService;
        }

        public async Task AckQueuedCommand(string messageId, DateTime? date)
        {
            _logger.LogInformation($"Acknowledging queued command with MessageId: {messageId}. ExecutedAt: {date}");

            var command = (await _deviceRepository.GetQueuedCommands(new GetQueuedCommandsModel()
            {
                MessageIds = [messageId]
            })).FirstOrDefault();

            if (command == null)
            {
                _logger.LogWarning($"Queued command with MessageId: {messageId} not found");
                return;
            }

            if (date != null)
            {
                command.ExecutedAt = date.Value;
                command.ExecutedAtUtc = _dateService.LocalToUtc(date.Value);
            }
            else
            {
                command.IsRemoved = true; // Indicates error
            }

            await _deviceRepository.SetQueuedCommand(command.DeviceId, command, true);

            _logger.LogInformation($"Successfully acknowledged queued command with MessageId: {messageId} for device: {command.DeviceId}");
        }
    }
}
