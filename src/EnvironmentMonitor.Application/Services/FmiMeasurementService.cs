using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        public FmiMeasurementService(
            IFmiWeatherClient weatherClient,
            IMeasurementRepository measurementRepository,
            IDeviceRepository deviceRepository,
            IDateService dateService,
            ILogger<FmiMeasurementService> logger)
        {
            _weatherClient = weatherClient;
            _measurementRepository = measurementRepository;
            _deviceRepository = deviceRepository;
            _dateService = dateService;
            _logger = logger;

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
                    "Temperature" => "t2m",
                    "Humidity" => "rh",
                    _ => t.Name.ToLowerInvariant()
                }, t => t);
        }

        public async Task<int> FetchAndStoreMeasurementsAsync(FetchFmiMeasurementsRequest request)
        {
            _logger.LogInformation($"Starting FMI measurement fetch for {request.Places.Count} places from {request.StartTimeUtc} to {request.EndTimeUtc}");
            
            var totalMeasurementsAdded = 0;

            foreach (var place in request.Places)
            {
                try
                {
                    _logger.LogInformation($"Fetching measurements for place: {place}");
                    
                    // Find or create device/sensor for this place
                    var sensor = await GetOrCreateSensorForPlace(place);
                    
                    if (sensor == null)
                    {
                        _logger.LogWarning($"Could not find or create sensor for place: {place}");
                        continue;
                    }

                    // Get the latest measurement timestamp for this sensor
                    var latestTimestamp = await GetLatestMeasurementTimestampForSensor(sensor.Id);
                    
                    if (latestTimestamp.HasValue)
                    {
                        _logger.LogInformation($"Latest measurement for sensor {sensor.Name} (ID: {sensor.Id}) is at {latestTimestamp.Value:u}");
                    }
                    else
                    {
                        _logger.LogInformation($"No existing measurements found for sensor {sensor.Name} (ID: {sensor.Id})");
                    }

                    // Request both temperature and relative humidity for this place
                    var series = await _weatherClient.GetSeriesAsync(
                        place,
                        request.StartTimeUtc,
                        request.EndTimeUtc,
                        new[] { "t2m", "rh" });

                    var measurements = new List<Measurement>();
                    var skippedCount = 0;

                    foreach (var seriesEntry in series)
                    {
                        if (!_typeMap.TryGetValue(seriesEntry.Key, out var type))
                        {
                            _logger.LogDebug($"Skipping unknown parameter type: {seriesEntry.Key}");
                            continue;
                        }

                        foreach (var dataPoint in seriesEntry.Value)
                        {
                            // Only add measurements that are newer than the latest in DB
                            if (latestTimestamp.HasValue && dataPoint.Time <= latestTimestamp.Value)
                            {
                                skippedCount++;
                                continue;
                            }

                            measurements.Add(new Measurement
                            {
                                SensorId = sensor.Id,
                                Sensor = sensor,
                                TypeId = type.Id,
                                Type = type,
                                Value = dataPoint.Value,
                                TimestampUtc = dataPoint.Time,
                                Timestamp = _dateService.UtcToLocal(dataPoint.Time),
                                CreatedAtUtc = DateTime.UtcNow,
                                CreatedAt = _dateService.CurrentTime()
                            });
                        }
                    }

                    if (skippedCount > 0)
                    {
                        _logger.LogInformation($"Skipped {skippedCount} measurements that already exist in database for place: {place}");
                    }

                    if (measurements.Any())
                    {
                        _logger.LogInformation($"Adding {measurements.Count} new measurements for place: {place}");
                        await _measurementRepository.AddMeasurements(measurements, true);
                        totalMeasurementsAdded += measurements.Count;
                    }
                    else
                    {
                        _logger.LogWarning($"No new measurements to add for place: {place}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error fetching measurements for place: {place}");
                }
            }

            _logger.LogInformation($"FMI measurement fetch completed. Total measurements added: {totalMeasurementsAdded}");
            return totalMeasurementsAdded;
        }


        public async Task PerformSync()
        {
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
                var sensors = await _deviceRepository.GetSensors(new GetSensorsModel
                {
                    DevicesModel = new GetDevicesModel
                    {
                        Ids = devices.Select(d => d.Id).ToList()
                    }
                });

                if (!sensors.Any())
                {
                    _logger.LogInformation("No sensors found for Ilmatieteenlaitos devices");
                    return;
                }

                _logger.LogInformation($"Found {sensors.Count} sensors for Ilmatieteenlaitos devices");
                // Extract unique place names from sensor names
                var places = sensors.Select(s => s.Name).Distinct().ToList();
                // Calculate time range - fetch last 4 days of data
                var endTimeUtc = _dateService.CurrentTime();
                var startTimeUtc = _dateService.CurrentTime().AddDays(-4);
                // Fetch and store measurements
                var request = new FetchFmiMeasurementsRequest
                {
                    Places = places,
                    StartTimeUtc = startTimeUtc,
                    EndTimeUtc = endTimeUtc
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

        private async Task<Sensor?> GetOrCreateSensorForPlace(string place)
        {
            // Try to find existing sensor with name matching the place
            var sensors = await _measurementRepository.Get(
                filter: m => m.Sensor.Name == place,
                includeProperties: "Sensor"
            );

            var sensor = sensors.Select(m => m.Sensor).FirstOrDefault();
            
            if (sensor != null)
            {
                _logger.LogDebug($"Found existing sensor for place: {place}");
                return sensor;
            }

            _logger.LogInformation($"No existing sensor found for place: {place}. Sensor creation not implemented in this version.");
            
            return null;
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
    }
}
