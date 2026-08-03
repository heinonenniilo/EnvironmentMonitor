using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class SyncService : ISyncService
    {
        private readonly ILogger<SyncService> _logger;
        private readonly SyncSettings _syncSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStatusVariableRepository _statusVariableRepository;
        private readonly IMeasurementService _measurementService;
        private readonly IUserService _userService;

        public SyncService(
            ILogger<SyncService> logger,
            SyncSettings syncSettings,
            IHttpClientFactory httpClientFactory,
            IDeviceRepository deviceRepository,
            IStatusVariableRepository statusVariableRepository,
            IDateService dateService,
            IUserService userService,
            IMeasurementService measurementService)
        {
            _logger = logger;
            _syncSettings = syncSettings;
            _httpClientFactory = httpClientFactory;
            _statusVariableRepository = statusVariableRepository;
            _measurementService = measurementService;
            _userService = userService;
        }

        public async Task<int> SyncMessages()
        {

            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }

            if (!_syncSettings.Enabled)
            {
                _logger.LogDebug("Sync is disabled in configuration");
                return 0;
            }

            if (string.IsNullOrEmpty(_syncSettings.Url))
            {
                _logger.LogWarning("Sync URL is not configured");
                return 0;
            }

            try
            {
                // Get the last synced device message ID
                var lastSyncVariable = await _statusVariableRepository.GetByKey(ApplicationConstants.SyncLastDeviceMessageIdKey);
                long lastSyncedId = 0;
                if (lastSyncVariable != null && long.TryParse(lastSyncVariable.Value, out var parsedId))
                {
                    lastSyncedId = parsedId;
                }

                _logger.LogInformation($"Starting sync from DeviceMessage ID: {lastSyncedId}");

                // Get unsynced device messages with optional filtering by communication channels
                var unsyncedMessages = await _statusVariableRepository.GetUnsyncedDeviceMessages(
                    lastSyncedId, 
                    _syncSettings.BatchSize, 
                    _syncSettings.ExcludedCommunicationChannels);

                if (!unsyncedMessages.Any())
                {
                    _logger.LogInformation("No unsynced messages found");
                    return 0;
                }

                _logger.LogInformation($"Found {unsyncedMessages.Count} unsynced messages");

                // Transform DeviceMessages to SaveMeasurementsDto
                var measurementDtos = new List<SaveMeasurementsDto>();
                foreach (var deviceMessage in unsyncedMessages)
                {
                    var dto = new SaveMeasurementsDto
                    {
                        DeviceId = deviceMessage.Device.DeviceIdentifier, // String
                        FirstMessage = deviceMessage.FirstMessage,
                        EnqueuedUtc = DateTime.SpecifyKind(deviceMessage.TimeStampUtc, DateTimeKind.Utc),
                        SequenceNumber = deviceMessage.SequenceNumber,
                        Uptime = deviceMessage.Uptime,
                        Identifier = deviceMessage.Identifier,
                        LoopCount = deviceMessage.LoopCount,
                        MessageCount = 0,
                        ExternalId = deviceMessage.Id,
                        Source = CommunicationChannels.Sync,
                        Measurements = deviceMessage.Measurements.Select(m => new AddMeasurementDto
                        {
                            SensorId = m.Sensor.SensorId, // Matches what device sends
                            SensorValue = m.Value,
                            TimestampUtc = DateTime.SpecifyKind(m.TimestampUtc, DateTimeKind.Utc),
                            Timestamp = DateTime.SpecifyKind(m.Timestamp, DateTimeKind.Unspecified),
                            TypeId = m.TypeId
                        }).ToList()
                    };
                    measurementDtos.Add(dto);
                }

                // Create sync request
                var syncRequest = new SyncMeasurementsRequest
                {
                    Measurements = measurementDtos
                };

                // Send to remote instance with proper authentication headers
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add(ApplicationConstants.ApiKeyHeaderName, _syncSettings.ApiKey);
                httpClient.DefaultRequestHeaders.Add(ApplicationConstants.SecretIdHeaderName, _syncSettings.SecretId);
                httpClient.DefaultRequestHeaders.Add(ApplicationConstants.SecretValueHeaderName, _syncSettings.SecretValue);

                var response = await httpClient.PostAsJsonAsync(_syncSettings.Url, syncRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Sync failed with status {response.StatusCode}: {errorContent}");
                    return 0;
                }

                _logger.LogInformation($"Successfully synced {unsyncedMessages.Count} messages");

                // Update last synced ID
                var highestSyncedId = unsyncedMessages.Max(m => m.Id);
                await _statusVariableRepository.SetValue(
                    ApplicationConstants.SyncLastDeviceMessageIdKey,
                    highestSyncedId.ToString());

                return unsyncedMessages.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sync operation");
                return 0;
            }
        }

        public async Task<SyncResultDto> ProcessIncomingSync(SyncMeasurementsRequest request)
        {

            if (request == null || request.Measurements == null || !request.Measurements.Any())
            {
                _logger.LogWarning("Received empty sync request");
                return new SyncResultDto { SyncedCount = 0, TotalReceived = 0 };

            }

            var isApiKeyUser = _userService.IsAdmin || _userService.Roles.Any(x => x.Equals(GlobalRoles.ApiKeyUser.ToString(), StringComparison.OrdinalIgnoreCase) );

            if (!isApiKeyUser && _userService.GetDevices().Count == 0)
            {
                _logger.LogWarning("Sync access denied: User lacks Admin or ApiKeyUser role and has no device claims");
                throw new UnauthorizedAccessException("Insufficient permissions for sync operation");
            }

            var totalReceived = request.Measurements.Count;
            var syncedCount = 0;

            _logger.LogInformation($"Processing incoming sync request with {totalReceived} measurements");

            foreach (var measurement in request.Measurements)
            {
                try
                {
                    measurement.Source = CommunicationChannels.Sync;
                    await _measurementService.AddMeasurements(measurement, skipStatusCheck: true);
                    syncedCount++;
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogWarning(ex, $"Access denied for synced measurement to device {measurement.DeviceId}");
                    // Continue processing other measurements - this one is skipped due to access rights
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing synced measurement for device {measurement.DeviceId}");
                    // Continue processing other measurements
                }
            }

            _logger.LogInformation($"Successfully processed {syncedCount} out of {totalReceived} synced measurements");

            return new SyncResultDto 
            { 
                SyncedCount = syncedCount, 
                TotalReceived = totalReceived 
            };
        }

    }
}
