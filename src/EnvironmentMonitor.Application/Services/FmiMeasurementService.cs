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
        private readonly IIdentifierGenerator _identifierGenerator;

        public FmiMeasurementService(
            IFmiWeatherClient weatherClient,
            IMeasurementRepository measurementRepository,
            IDeviceRepository deviceRepository,
            IDateService dateService,
            IUserService userService,
            IIdentifierGenerator identifierGenerator,
            ILogger<FmiMeasurementService> logger)
        {
            _weatherClient = weatherClient;
            _measurementRepository = measurementRepository;
            _deviceRepository = deviceRepository;
            _dateService = dateService;
            _logger = logger;
            _userService = userService;       
            _identifierGenerator = identifierGenerator;
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

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel
            {
                Ids = [request.Device.Id],
                GetSensors = true
            })).FirstOrDefault();

            if (device == null || device.CommunicationChannelId != (int)CommunicationChannels.IlmatieteenLaitos)
            {
                _logger.LogWarning($"Device {request.Device.Id} is not an Ilmatieteenlaitos device or has no sensors");
                return 0;
            }

            var sensors = device.Sensors;

            _logger.LogInformation($"Starting FMI measurement fetch for {sensors.Count} sensors");
            
            var totalMeasurementsAdded = 0;
            // Group sensors by place name
            var sensorsByPlace = sensors
                .GroupBy(s => s.Name)
                .ToDictionary(g => g.Key, g => g.ToList());

            var places = sensorsByPlace.Keys.ToList();

            if (!places.Any())
            {
                _logger.LogWarning("No places to fetch measurements for");
                return 0;
            }
            var endTimeUtc = request.EndTimeUtc;
            var startTimeUtc = request.StartTimeUtc;
            _logger.LogInformation($"Fetching measurements for {places.Count} unique places from FMI (last 3 days): {string.Join(", ", places)}");
            // Get latest timestamp for each sensor before making API call
            var sensorLatestTimestamps = new Dictionary<int, DateTime?>();
            foreach (var sensor in sensors)
            {
                var sensorLatest = await GetLatestMeasurementTimestampForSensor(sensor.Id);
                sensorLatestTimestamps[sensor.Id] = sensorLatest;               
            }

            var apiCallStartUtc = sensorLatestTimestamps.Values.Any(x => !x.HasValue) ? null : sensorLatestTimestamps.Values.Min();

            var allMeasurementsByPlace = await _weatherClient.GetSeriesAsync(
                places,
                apiCallStartUtc != null ? apiCallStartUtc.Value: startTimeUtc,
                endTimeUtc,
                new[] { IlmatieteenlaitosConstants.TemperatureTypeKey, IlmatieteenlaitosConstants.HumidityTypeKey });

            _logger.LogInformation($"Received data from FMI for {allMeasurementsByPlace.Count} places");

            // Now loop through sensors and find matching measurements in the result set
            var measurements = new List<Measurement>();
            var skippedCount = 0;
            var invalidValueCount = 0;

            foreach (var sensor in sensors)
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
                            CreatedAt = _dateService.CurrentTime()
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

            if (measurements.Count != 0)
            {
                // Add device message
                var dateNow = _dateService.CurrentTime();
                DeviceMessage? deviceMessage = null;
                deviceMessage = new DeviceMessage()
                {
                    TimeStamp = dateNow,
                    TimeStampUtc = _dateService.LocalToUtc(dateNow),
                    DeviceId = request.Device.Id,
                    Created = _dateService.CurrentTime(),
                    SourceId = (int)MeasurementSourceTypes.Ilmatieteenlaitos,
                    Identifier = _identifierGenerator.GenerateId()
                };
                await _measurementRepository.AddDeviceMessage(deviceMessage, false);

                _logger.LogInformation($"Adding {measurements.Count} new measurements to database");
                await _measurementRepository.AddMeasurements(measurements, true, deviceMessage);
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
            var devices = await _deviceRepository.GetDevices(new GetDevicesModel
            {
                CommunicationChannelIds = new List<int> { (int)CommunicationChannels.IlmatieteenLaitos }
            });

            if (!devices.Any())
            {
                _logger.LogInformation("No Ilmatieteenlaitos devices found to sync");
                return;
            }

            foreach (var device in devices)
            {
                _logger.LogInformation($"Found Ilmatieteenlaitos device: ID={device.Id}, Identifier={device.Identifier}");
                var request = new FetchFmiMeasurementsRequest
                {
                    Device = device,
                    StartTimeUtc = DateTime.UtcNow.AddDays(-6),
                    EndTimeUtc = DateTime.UtcNow
                };
                var totalMeasurements = await FetchAndStoreMeasurementsAsync(request);
                _logger.LogInformation($"FMI sync completed successfully for '{device.Name}'. Total measurements added: {totalMeasurements}");
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
