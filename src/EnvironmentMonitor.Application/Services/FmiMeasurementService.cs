using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class FmiMeasurementService : IFmiMeasurementService
    {
        private readonly IFmiWeatherClient _weatherClient;
        private readonly IMeasurementRepository _measurementRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IDateService _dateService;
        private readonly ILogger<FmiMeasurementService> _logger;
        private readonly IReadOnlyDictionary<string, MeasurementType> _typeMap;
        private readonly IUserService _userService;

        public FmiMeasurementService(
            IFmiWeatherClient weatherClient,
            IMeasurementRepository measurementRepository,
            IDeviceRepository deviceRepository,
            IDateService dateService,
            IUserService userService,
            ILogger<FmiMeasurementService> logger)
        {
            _weatherClient = weatherClient;
            _measurementRepository = measurementRepository;
            _deviceRepository = deviceRepository;
            _dateService = dateService;
            _logger = logger;
            _userService = userService;           
            // Define measurement types and map FMI parameter codes to them
            var types = new List<MeasurementType>
            {
                new MeasurementType { Id = (int)MeasurementTypes.Temperature, Name = "Temperature", Unit = "C" },
                new MeasurementType { Id = (int)MeasurementTypes.Humidity, Name = "Humidity", Unit = "%" }
            };            
            // Map FMI parameter codes (t2m for temperature, rh for humidity) to types
            _typeMap = types.ToDictionary(
                t => t.Name switch
                {
                    "Temperature" => IlmatieteenlaitosConstants.TemperatureTypeKey,
                    "Humidity" => IlmatieteenlaitosConstants.HumidityTypeKey,
                    _ => t.Name.ToLowerInvariant()
                }, t => t);
        }

        public async Task<int> FetchAndStoreMeasurementsAsync(FetchFmiMeasurementsRequest request)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }

            _logger.LogInformation($"Starting FMI measurement fetch for {request.Sensors.Count} sensors");
            
            var totalMeasurementsAdded = 0;
            // Group sensors by place name
            var sensorsByPlace = request.Sensors
                .GroupBy(s => s.Name)
                .ToDictionary(g => g.Key, g => g.ToList());

            var places = sensorsByPlace.Keys.ToList();

            if (!places.Any())
            {
                _logger.LogWarning("No places to fetch measurements for");
                return 0;
            }
            // Last 6 days
            var endTimeUtc = DateTime.UtcNow;
            var startTimeUtc = endTimeUtc.AddDays(-6);
            _logger.LogInformation($"Fetching measurements for {places.Count} unique places from FMI (last 3 days): {string.Join(", ", places)}");
            // Get latest timestamp for each sensor before making API call
            var sensorLatestTimestamps = new Dictionary<int, DateTime?>();
            foreach (var sensor in request.Sensors)
            {
                var sensorLatest = await GetLatestMeasurementTimestampForSensor(sensor.Id);
                sensorLatestTimestamps[sensor.Id] = sensorLatest;
                
                if (sensorLatest.HasValue)
                {
                    _logger.LogDebug($"Latest measurement for sensor {sensor.Name} (ID: {sensor.Id}) is at {sensorLatest.Value:u}");
                }
            }
            Dictionary<string, Dictionary<string, List<(DateTime Time, double Value)>>> allMeasurementsByPlace;
            _logger.LogInformation($"Making single API call to FMI for all {places.Count} places");

            allMeasurementsByPlace = await _weatherClient.GetSeriesAsync(
                places,
                startTimeUtc,
                endTimeUtc,
                new[] { IlmatieteenlaitosConstants.TemperatureTypeKey, IlmatieteenlaitosConstants.HumidityTypeKey });

            _logger.LogInformation($"Received data from FMI for {allMeasurementsByPlace.Count} places");

            // Now loop through sensors and find matching measurements in the result set
            var measurements = new List<Measurement>();
            var skippedCount = 0;
            var invalidValueCount = 0;

            foreach (var sensor in request.Sensors)
            {
                var place = sensor.Name;

                // Try exact match first
                if (!allMeasurementsByPlace.TryGetValue(place, out var series))
                {
                    // Try to find a place that contains the sensor name (case-insensitive)
                    var matchingPlace = allMeasurementsByPlace.Keys
                        .FirstOrDefault(p => p.Contains(place, StringComparison.OrdinalIgnoreCase));

                    if (matchingPlace != null)
                    {
                        series = allMeasurementsByPlace[matchingPlace];
                        _logger.LogInformation($"Matched sensor '{place}' (ID: {sensor.Id}) to FMI place: '{matchingPlace}'");
                    }
                    else
                    {
                        // Try reverse - sensor name contains the place name
                        matchingPlace = allMeasurementsByPlace.Keys
                            .FirstOrDefault(p => place.Contains(p, StringComparison.OrdinalIgnoreCase));

                        if (matchingPlace != null)
                        {
                            series = allMeasurementsByPlace[matchingPlace];
                            _logger.LogInformation($"Matched sensor '{place}' (ID: {sensor.Id}) to FMI place: '{matchingPlace}'");
                        }
                        else
                        {
                            _logger.LogWarning($"No matching data available for place: {place} (Sensor ID: {sensor.Id}). Available places: {string.Join(", ", allMeasurementsByPlace.Keys)}");
                            continue;
                        }
                    }
                }

                var latestTimestamp = sensorLatestTimestamps[sensor.Id];
                var dateNow = _dateService.CurrentTime();
                DeviceMessage? deviceMessage = null;
                if (latestTimestamp == null || series.Values.Any(value => value.Any(d => d.Time > latestTimestamp)))
                {
                    deviceMessage = new DeviceMessage()
                    {
                        TimeStamp = dateNow,
                        TimeStampUtc = _dateService.LocalToUtc(dateNow),
                        DeviceId = sensor.DeviceId,
                        Created = _dateService.CurrentTime(),
                        SourceId = (int)MeasurementSourceTypes.Ilmatieteenlaitos,
                    };
                    await _measurementRepository.AddDeviceMessage(deviceMessage, false);
                }             

                foreach (var seriesEntry in series)
                {
                    if (!_typeMap.TryGetValue(seriesEntry.Key, out var type))
                    {
                        continue;
                    }

                    foreach (var dataPoint in seriesEntry.Value)
                    {
                        // Only add measurements that are newer than the latest in DB for THIS sensor
                        if (latestTimestamp.HasValue && dataPoint.Time <= latestTimestamp.Value)
                        {
                            skippedCount++;
                            continue;
                        }

                        // Validate the value - skip NaN, Infinity, and other invalid values
                        if (!IsValidMeasurementValue(dataPoint.Value))
                        {
                            _logger.LogWarning($"Skipping invalid value {dataPoint.Value} for sensor {sensor.Name} (ID: {sensor.Id}) at {dataPoint.Time:u}");
                            invalidValueCount++;
                            continue;
                        }

                        measurements.Add(new Measurement
                        {
                            SensorId = sensor.Id,
                            TypeId = type.Id,
                            Value = dataPoint.Value,
                            TimestampUtc = dataPoint.Time,
                            Timestamp = _dateService.UtcToLocal(dataPoint.Time),
                            CreatedAtUtc = DateTime.UtcNow,
                            CreatedAt = _dateService.CurrentTime(),
                            DeviceMessage = deviceMessage,
                        });
                    }
                }
            }

            if (skippedCount > 0)
            {
                _logger.LogInformation($"Skipped {skippedCount} measurements (older than latest per sensor)");
            }

            if (invalidValueCount > 0)
            {
                _logger.LogWarning($"Skipped {invalidValueCount} measurements due to invalid values (NaN or Infinity)");
            }

            if (measurements.Any())
            {
                _logger.LogInformation($"Adding {measurements.Count} new measurements to database");
                await _measurementRepository.AddMeasurements(measurements, true);
                totalMeasurementsAdded = measurements.Count;
            }
            else
            {
                _logger.LogInformation($"No new measurements to add");
            }

            _logger.LogInformation($"FMI measurement fetch completed. Total measurements added: {totalMeasurementsAdded}");
            return totalMeasurementsAdded;
        }


        public async Task PerformSync()
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }

            _logger.LogInformation("Starting FMI sync for Ilmatieteenlaitos devices");
            
            try
            {
                // Fetch all devices with CommunicationChannelId = Ilmatieteenlaitos
                var devices = await _deviceRepository.GetDevices(new GetDevicesModel
                {
                    CommunicationChannelIds = new List<int> { (int)CommunicationChannels.IlmatieteenLaitos }
                });

                if (!devices.Any())
                {
                    _logger.LogInformation("No Ilmatieteenlaitos devices found to sync");
                    return;
                }

                _logger.LogInformation($"Found {devices.Count} Ilmatieteenlaitos devices to sync");

                // Get all sensors for these devices
                var sensorsExtended = await _deviceRepository.GetSensors(new GetSensorsModel
                {
                    DevicesModel = new GetDevicesModel
                    {
                        Ids = devices.Select(d => d.Id).ToList()
                    }
                });

                if (!sensorsExtended.Any())
                {
                    _logger.LogInformation("No sensors found for Ilmatieteenlaitos devices");
                    return;
                }

                _logger.LogInformation($"Found {sensorsExtended.Count} sensors for Ilmatieteenlaitos devices");

                // Convert SensorExtended to Sensor entities
                var sensors = sensorsExtended.Select(s => new Sensor
                {
                    Id = s.Id,
                    Name = s.Name,
                    Identifier = s.Identifier,
                    DeviceId = s.DeviceId,
                    SensorId = s.SensorId,
                    TypeId = s.TypeId,
                    ScaleMin = s.ScaleMin,
                    ScaleMax = s.ScaleMax
                }).ToList();

                // Fetch and store measurements (always fetches last 3 days from FMI)
                var request = new FetchFmiMeasurementsRequest
                {
                    Sensors = sensors,
                    StartTimeUtc = DateTime.UtcNow.AddDays(-3), // Not used, but kept for interface compatibility
                    EndTimeUtc = DateTime.UtcNow // Not used, but kept for interface compatibility
                };

                var totalMeasurements = await FetchAndStoreMeasurementsAsync(request);

                _logger.LogInformation($"FMI sync completed successfully. Total measurements added: {totalMeasurements}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during FMI sync");
                throw;
            }
        }

        private async Task<DateTime?> GetLatestMeasurementTimestampForSensor(int sensorId)
        {
            try
            {
                var latestMeasurements = await _measurementRepository.Get(
                    filter: m => m.SensorId == sensorId,
                    orderBy: q => q.OrderByDescending(m => m.TimestampUtc)
                );

                var latestMeasurement = latestMeasurements.FirstOrDefault();
                return latestMeasurement?.TimestampUtc;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error getting latest measurement timestamp for sensor {sensorId}, will fetch all measurements");
                return null;
            }
        }

        private static bool IsValidMeasurementValue(double value)
        {
            // Check for NaN, positive/negative infinity
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return false;
            }

            // Optionally check for extreme values that might cause issues
            // SQL Server float range is approximately ±1.79E+308
            if (Math.Abs(value) > 1.0E+308)
            {
                return false;
            }

            return true;
        }
    }
}
