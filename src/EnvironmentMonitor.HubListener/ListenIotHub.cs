using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;
using Microsoft.Azure.WebJobs;
using System.Text;
using Microsoft.Extensions.Logging;
using EnvironmentMonitor.Application.Interfaces;
using System.Threading.Tasks;
using System.Text.Json;
using EnvironmentMonitor.Application.DTOs;
using System;

namespace EnvironmentMonitor.HubListener
{
    public class ListenIotHub
    {
        readonly ILogger<ListenIotHub> _logger;
        private readonly IMeasurementService _measurementService;
        public ListenIotHub(ILogger<ListenIotHub> logger, IMeasurementService measurementService)
        {
            _logger = logger;
            _measurementService = measurementService;
        }
        [FunctionName("ListenIotHub")]
        public async Task Run([IoTHubTrigger("$Default", Connection = "hubconnectionstring")] Microsoft.Azure.EventHubs.EventData message, ILogger log)
        {
            var bodyString = Encoding.UTF8.GetString(message.Body);
            MeasurementDto objectToInsert;
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                objectToInsert = JsonSerializer.Deserialize<MeasurementDto>(bodyString, options);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error");
                throw;
            }
            foreach (var item in objectToInsert.Measurements)
            {
                item.TimeStamp = message.SystemProperties.EnqueuedTimeUtc;
            }
            try
            {
                await _measurementService.AddMeasurements(objectToInsert);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error");
                throw;
            }
        }
    }
}