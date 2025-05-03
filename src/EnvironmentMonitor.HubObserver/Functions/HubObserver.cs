using System;
using System.Text.Json;
using System.Text;
using Azure.Messaging.EventHubs;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EnvironmentMonitor.HubObserver.Functions
{
    public class HubObserver
    {
        private readonly ILogger<HubObserver> _logger;
        private readonly IMeasurementService _measurementService;

        public HubObserver(ILogger<HubObserver> logger, IMeasurementService measurementService)
        {
            _logger = logger;
            _measurementService = measurementService;
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
                try
                {
                    await _measurementService.AddMeasurements(objectToInsert);
                    processedMessaged++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Adding measurements failed");
                    throw;
                }
            }
            _logger.LogInformation($"Total of {processedMessaged} processed");
        }
    }
}
