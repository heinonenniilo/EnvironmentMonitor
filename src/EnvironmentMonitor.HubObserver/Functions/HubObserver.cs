using System;
using System.Text.Json;
using System.Text;
using Azure.Messaging.EventHubs;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Enums;

namespace EnvironmentMonitor.HubObserver.Functions
{
    public class HubObserver
    {
        private readonly ILogger<HubObserver> _logger;
        private readonly IMeasurementService _measurementService;
        private readonly IQueueClient _queueClient;
        private readonly IDeviceService _deviceService;

        private const int FirstMessageLimitInMinutes = 5;

        public HubObserver(ILogger<HubObserver> logger, IMeasurementService measurementService, IQueueClient queueClient, IDeviceService deviceService)
        {
            _logger = logger;
            _measurementService = measurementService;
            _queueClient = queueClient;
            _deviceService = deviceService;
        }

        [Function(nameof(HubObserver))]
        public async Task Run([EventHubTrigger("$Default", Connection = "hubconnectionstring", ConsumerGroup = "%ConsumerGroup%")] EventData[] events)
        {
            if (events?.Length > 0)
            {
                _logger.LogInformation($"Start processing {events.Length} messages.");
            }
            else
            {
                _logger.LogWarning("No messages to process!");
                return;
            }
            var processedMessaged = 0;

            foreach (EventData message in events)
            {
                var bodyString = Encoding.UTF8.GetString(message.EventBody);
                if (bodyString == null)
                {
                    _logger.LogError($"Body string NULL");
                    continue;
                }
                SaveMeasurementsDto? objectToInsert = null;
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    objectToInsert = JsonSerializer.Deserialize<SaveMeasurementsDto>(bodyString, options);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize JSON");
                    throw;
                }

                if (objectToInsert == null)
                {
                    _logger.LogError("Null JSON object");
                    return;
                }

                foreach (var item in objectToInsert.Measurements)
                {
                    item.TimestampUtc = message.EnqueuedTime.UtcDateTime;
                }

                objectToInsert.EnqueuedUtc = message.EnqueuedTime.UtcDateTime;
                objectToInsert.SequenceNumber = message.SequenceNumber;
                objectToInsert.Source = CommunicationChannels.IotHub;
                try
                {
                    await _measurementService.AddMeasurements(objectToInsert);
                    processedMessaged++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Adding measurements failed");
                }

                if (objectToInsert.FirstMessage)
                {
                    if (objectToInsert.EnqueuedUtc > DateTime.UtcNow.AddMinutes(-1 * FirstMessageLimitInMinutes))
                    {
                        try
                        {
                            var device = await _deviceService.GetDevice(objectToInsert.DeviceId, AccessLevels.Write);
                            var queueMessage = new Domain.Models.DeviceQueueMessage
                            {
                                DeviceIdentifier = device.Identifier,
                                MessageTypeId = (int)QueuedMessages.SendDeviceAttributes
                            };
                            var messageJson = JsonSerializer.Serialize(queueMessage);
                            _logger.LogInformation($"First message detected for device {device.Identifier}, sending to queue");
                            await _queueClient.SendMessage(messageJson);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to send device attributes to device with id (string) {objectToInsert.DeviceId}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"First message received for device ({objectToInsert.DeviceId}). It was enqueued {objectToInsert.EnqueuedUtc} which was over {FirstMessageLimitInMinutes} mins ago");
                    }
                }
            }
            _logger.LogInformation($"Total of {processedMessaged} processed");
        }
    }
}
