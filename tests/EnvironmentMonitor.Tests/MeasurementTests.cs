using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Infrastructure.Data;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;
using Assert = NUnit.Framework.Assert;

namespace EnvironmentMonitor.Tests
{
    [TestFixture]
    public class MeasurementTests : BaseIntegrationTest
    {
        [Test]
        public async Task GetMeasurementsByLocation_ReturnsCorrectResults()
        {
            // Arrange
            var model = await PrepareDatabase();
            
            var sensor1 = model.DeviceInLocation.Sensors.First();
            var sensor2 = model.DeviceInLocationWithNoAccess.Sensors.First();

            // Add LocationSensors and Measurements for Location 1
            await AddLocationSensorsAndMeasurements(
                model.Location.Id,
                sensor1.Id,
                model.DeviceInLocation.Id,
                "Location1-Sensor1",
                1,
                new List<(double, DateTime)>
                {
                    (22.5, DateTime.Now.AddHours(-2)),
                    (23.0, DateTime.Now.AddHours(-1))
                }
            );

            // Add LocationSensors and Measurements for Location 2
            await AddLocationSensorsAndMeasurements(
                model.LocationWithNoDefinedAccess.Id,
                sensor2.Id,
                model.DeviceInLocationWithNoAccess.Id,
                "Location2-Sensor1",
                1,
                new List<(double, DateTime)>
                {
                    (18.5, DateTime.Now.AddHours(-2)),
                    (19.0, DateTime.Now.AddHours(-1))
                }
            );
            
            await LoginAsync(AdminUserName, AdminPassword);

            var queryParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("LocationIdentifiers", model.Location.Identifier.ToString()),
                new KeyValuePair<string, string>("From", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"))
            };

            var apiPath = "/api/measurements/bylocation";
            var clientPath = QueryHelpers.AddQueryString(apiPath, queryParams);

            // Act
            var response = await _client.GetAsync(clientPath);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<MeasurementsByLocationModel>(content);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(content, Is.Not.Null);
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Measurements, Is.Not.Null);
                
                // Should only contain measurements from the requested location
                Assert.That(result.Measurements.Count, Is.EqualTo(1));
                Assert.That(result.Measurements[0].Identifier, Is.EqualTo(model.Location.Identifier));
                
                // Verify measurements exist
                Assert.That(result.Measurements[0].Measurements, Is.Not.Empty);
                
                var sensorMeasurements = result.Measurements[0].Measurements.First();
                Assert.That(sensorMeasurements.Measurements.Count, Is.EqualTo(2));
                
                // Verify values are correct (should be 22.5 and 23.0)
                var values = sensorMeasurements.Measurements.Select(m => m.SensorValue).OrderBy(v => v).ToList();
                Assert.That(values[0], Is.EqualTo(22.5));
                Assert.That(values[1], Is.EqualTo(23.0));
            });
        }

        [Test]
        public async Task GetMeasurementsByLocation_WithSensorFilter_ReturnsFilteredResults()
        {
            // Arrange
            var model = await PrepareDatabase();

            // Get the two temperature sensors from the device in location
            var temperatureSensor1 = model.DeviceInLocation.Sensors.First(s => s.Name == "Temperature-Sensor-01");
            var temperatureSensor2 = model.DeviceInLocation.Sensors.First(s => s.Name == "Temperature-Sensor-02");

            // Add LocationSensors and Measurements for both sensors
            await AddLocationSensorsAndMeasurements(
                model.Location.Id,
                temperatureSensor1.Id,
                model.DeviceInLocation.Id,
                "Location1-Temperature-01",
                1,
                new List<(double, DateTime)>
                {
                    (22.5, DateTime.Now.AddHours(-2)),
                    (23.0, DateTime.Now.AddHours(-1)),
                    (23.5, DateTime.Now.AddMinutes(-30))
                }
            );

            await AddLocationSensorsAndMeasurements(
                model.Location.Id,
                temperatureSensor2.Id,
                model.DeviceInLocation.Id,
                "Location1-Temperature-02",
                1,
                new List<(double, DateTime)>
                {
                    (65.0, DateTime.Now.AddHours(-2)),
                    (67.0, DateTime.Now.AddHours(-1)),
                    (68.5, DateTime.Now.AddMinutes(-30))
                }
            );

            await LoginAsync(AdminUserName, AdminPassword);

            // Query with location and specific sensor filter (should return only first temperature sensor)
            var queryParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("LocationIdentifiers", model.Location.Identifier.ToString()),
                new KeyValuePair<string, string>("SensorIdentifiers", temperatureSensor1.Identifier.ToString()),
                new KeyValuePair<string, string>("From", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"))
            };

            var apiPath = "/api/measurements/bylocation";
            var clientPath = QueryHelpers.AddQueryString(apiPath, queryParams);

            // Act
            var response = await _client.GetAsync(clientPath);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<MeasurementsByLocationModel>(content);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Measurements, Is.Not.Null);
                Assert.That(result.Measurements.Count, Is.EqualTo(1), "Should return one location");
                Assert.That(result.Measurements[0].Measurements.Count, Is.EqualTo(1), "Should return measurements from only the filtered sensor");

                // Verify only first temperature sensor measurements are returned
                var sensorMeasurements = result.Measurements[0].Measurements.First();
                Assert.That(sensorMeasurements.SensorIdentifier, Is.EqualTo(temperatureSensor1.Identifier), "Should only include the filtered sensor");
                Assert.That(sensorMeasurements.Measurements.Count, Is.EqualTo(3), "Should have 3 measurements");
                
                // Verify the correct temperature values are returned
                var values = sensorMeasurements.Measurements.Select(m => m.SensorValue).OrderBy(v => v).ToList();
                Assert.That(values[0], Is.EqualTo(22.5));
                Assert.That(values[1], Is.EqualTo(23.0));
                Assert.That(values[2], Is.EqualTo(23.5));
            });
        }

        [Test]
        public async Task GetMeasurementsBySensor_WithLatestOnly_ReturnsOnlyLatestMeasurement()
        {
            // Arrange
            var model = await PrepareDatabase();
            var sensor1 = model.DeviceInLocation.Sensors.First(s => s.Name == "Temperature-Sensor-01");
            var sensor2 = model.DeviceInLocation.Sensors.First(s => s.Name == "Temperature-Sensor-02");

            // Add measurements for sensor1 with TypeId 1 (Temperature)
            // Add measurements for sensor2 with TypeId 2 (Humidity) and TypeId 1
            using (var scope = _factory.Services.CreateScope())
            {
                var measurementDbContext = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();

                var measurements = new List<Measurement>
                {
                    // Sensor 1 - TypeId 1 (Temperature) - 3 measurements
                    new Measurement
                    {
                        SensorId = sensor1.Id,
                        TypeId = 1,
                        Value = 20.0,
                        Timestamp = DateTime.Now.AddHours(-5),
                        TimestampUtc = DateTime.UtcNow.AddHours(-5),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new Measurement
                    {
                        SensorId = sensor1.Id,
                        TypeId = 1,
                        Value = 21.5,
                        Timestamp = DateTime.Now.AddHours(-3),
                        TimestampUtc = DateTime.UtcNow.AddHours(-3),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new Measurement
                    {
                        SensorId = sensor1.Id,
                        TypeId = 1,
                        Value = 23.5,
                        Timestamp = DateTime.Now.AddMinutes(-30),
                        TimestampUtc = DateTime.UtcNow.AddMinutes(-30),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    // Sensor 2 - TypeId 1 (Temperature) - 3 measurements
                    new Measurement
                    {
                        SensorId = sensor2.Id,
                        TypeId = 1,
                        Value = 18.0,
                        Timestamp = DateTime.Now.AddHours(-4),
                        TimestampUtc = DateTime.UtcNow.AddHours(-4),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new Measurement
                    {
                        SensorId = sensor2.Id,
                        TypeId = 1,
                        Value = 19.5,
                        Timestamp = DateTime.Now.AddHours(-2),
                        TimestampUtc = DateTime.UtcNow.AddHours(-2),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new Measurement
                    {
                        SensorId = sensor2.Id,
                        TypeId = 1,
                        Value = 20.5,
                        Timestamp = DateTime.Now.AddMinutes(-45),
                        TimestampUtc = DateTime.UtcNow.AddMinutes(-45),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    // Sensor 2 - TypeId 2 (Humidity) - 3 measurements
                    new Measurement
                    {
                        SensorId = sensor2.Id,
                        TypeId = 2,
                        Value = 60.0,
                        Timestamp = DateTime.Now.AddHours(-4),
                        TimestampUtc = DateTime.UtcNow.AddHours(-4),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new Measurement
                    {
                        SensorId = sensor2.Id,
                        TypeId = 2,
                        Value = 65.0,
                        Timestamp = DateTime.Now.AddHours(-2),
                        TimestampUtc = DateTime.UtcNow.AddHours(-2),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new Measurement
                    {
                        SensorId = sensor2.Id,
                        TypeId = 2,
                        Value = 68.5,
                        Timestamp = DateTime.Now.AddMinutes(-20),
                        TimestampUtc = DateTime.UtcNow.AddMinutes(-20),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    }
                };

                measurementDbContext.Measurements.AddRange(measurements);
                await measurementDbContext.SaveChangesAsync();
            }

            await LoginAsync(AdminUserName, AdminPassword);

            // Query with LatestOnly parameter for both sensors
            var queryParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("SensorIdentifiers", sensor1.Identifier.ToString()),
                new KeyValuePair<string, string>("SensorIdentifiers", sensor2.Identifier.ToString()),
                new KeyValuePair<string, string>("LatestOnly", "true")
            };

            var apiPath = "/api/measurements/bysensor";
            var clientPath = QueryHelpers.AddQueryString(apiPath, queryParams);

            // Act
            var response = await _client.GetAsync(clientPath);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<MeasurementsBySensorModel>(content);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Measurements, Is.Not.Null);
                Assert.That(result.Measurements.Count, Is.EqualTo(2), "Should return two sensors");

                // Verify Sensor 1 - should have 1 measurement (latest TypeId 1)
                var sensor1Measurements = result.Measurements.FirstOrDefault(m => m.SensorIdentifier == sensor1.Identifier);
                Assert.That(sensor1Measurements, Is.Not.Null, "Sensor 1 should be in results");
                Assert.That(sensor1Measurements.Measurements.Count, Is.EqualTo(1), "Sensor 1 should have only 1 latest measurement");
                
                var sensor1LatestMeasurement = sensor1Measurements.Measurements.First();
                Assert.That(sensor1LatestMeasurement.TypeId, Is.EqualTo(1), "Sensor 1 latest should be TypeId 1");
                Assert.That(sensor1LatestMeasurement.SensorValue, Is.EqualTo(23.5), "Sensor 1 latest value should be 23.5");

                // Verify Sensor 2 - should have 2 measurements (latest for each TypeId)
                var sensor2Measurements = result.Measurements.FirstOrDefault(m => m.SensorIdentifier == sensor2.Identifier);
                Assert.That(sensor2Measurements, Is.Not.Null, "Sensor 2 should be in results");
                Assert.That(sensor2Measurements.Measurements.Count, Is.EqualTo(2), "Sensor 2 should have 2 measurements (one per TypeId)");

                // Verify latest TypeId 1 measurement for Sensor 2
                var sensor2Type1 = sensor2Measurements.Measurements.FirstOrDefault(m => m.TypeId == 1);
                Assert.That(sensor2Type1, Is.Not.Null, "Sensor 2 should have TypeId 1 measurement");
                Assert.That(sensor2Type1.SensorValue, Is.EqualTo(20.5), "Sensor 2 TypeId 1 latest value should be 20.5");

                // Verify latest TypeId 2 measurement for Sensor 2
                var sensor2Type2 = sensor2Measurements.Measurements.FirstOrDefault(m => m.TypeId == 2);
                Assert.That(sensor2Type2, Is.Not.Null, "Sensor 2 should have TypeId 2 measurement");
                Assert.That(sensor2Type2.SensorValue, Is.EqualTo(68.5), "Sensor 2 TypeId 2 latest value should be 68.5");
            });
        }

        [Test]
        public async Task GetMeasurementsBySensor_WithDeviceIdentifiers_ReturnsMeasurementsForDeviceSensors()
        {
            // Arrange
            var model = await PrepareDatabase();
            
            var device1 = model.DeviceInLocation;
            var device2 = model.DeviceInLocationWithNoAccess;

            // Get sensors from both devices
            var device1Sensor1 = device1.Sensors.First(s => s.Name == "Temperature-Sensor-01");
            var device1Sensor2 = device1.Sensors.First(s => s.Name == "Temperature-Sensor-02");
            var device2Sensor1 = device2.Sensors.First();

            // Add measurements for Device 1 sensors
            using (var scope = _factory.Services.CreateScope())
            {
                var measurementDbContext = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();

                var measurements = new List<Measurement>
                {
                    // Device 1 - Sensor 1
                    new Measurement
                    {
                        SensorId = device1Sensor1.Id,
                        TypeId = 1,
                        Value = 22.5,
                        Timestamp = DateTime.Now.AddHours(-2),
                        TimestampUtc = DateTime.UtcNow.AddHours(-2),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new Measurement
                    {
                        SensorId = device1Sensor1.Id,
                        TypeId = 1,
                        Value = 23.0,
                        Timestamp = DateTime.Now.AddHours(-1),
                        TimestampUtc = DateTime.UtcNow.AddHours(-1),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    // Device 1 - Sensor 2
                    new Measurement
                    {
                        SensorId = device1Sensor2.Id,
                        TypeId = 1,
                        Value = 65.0,
                        Timestamp = DateTime.Now.AddHours(-2),
                        TimestampUtc = DateTime.UtcNow.AddHours(-2),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new Measurement
                    {
                        SensorId = device1Sensor2.Id,
                        TypeId = 1,
                        Value = 67.5,
                        Timestamp = DateTime.Now.AddHours(-1),
                        TimestampUtc = DateTime.UtcNow.AddHours(-1),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    // Device 2 - Sensor 1 (should NOT be returned)
                    new Measurement
                    {
                        SensorId = device2Sensor1.Id,
                        TypeId = 1,
                        Value = 18.0,
                        Timestamp = DateTime.Now.AddHours(-2),
                        TimestampUtc = DateTime.UtcNow.AddHours(-2),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new Measurement
                    {
                        SensorId = device2Sensor1.Id,
                        TypeId = 1,
                        Value = 19.0,
                        Timestamp = DateTime.Now.AddHours(-1),
                        TimestampUtc = DateTime.UtcNow.AddHours(-1),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    }
                };

                measurementDbContext.Measurements.AddRange(measurements);
                await measurementDbContext.SaveChangesAsync();
            }

            await LoginAsync(AdminUserName, AdminPassword);

            // Query with DeviceIdentifiers for Device 1 only
            var queryParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("DeviceIdentifiers", device1.Identifier.ToString()),
                new KeyValuePair<string, string>("From", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"))
            };

            var apiPath = "/api/measurements/bysensor";
            var clientPath = QueryHelpers.AddQueryString(apiPath, queryParams);

            // Act
            var response = await _client.GetAsync(clientPath);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<MeasurementsBySensorModel>(content);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Measurements, Is.Not.Null);
                
                // Should return measurements from 2 sensors of Device 1
                Assert.That(result.Measurements.Count, Is.EqualTo(2), "Should return measurements from both sensors of Device 1");

                // Verify Device 1 Sensor 1 measurements
                var sensor1Measurements = result.Measurements.FirstOrDefault(m => m.SensorIdentifier == device1Sensor1.Identifier);
                Assert.That(sensor1Measurements, Is.Not.Null, "Device 1 Sensor 1 should be in results");
                Assert.That(sensor1Measurements.Measurements.Count, Is.EqualTo(2), "Device 1 Sensor 1 should have 2 measurements");
                
                var sensor1Values = sensor1Measurements.Measurements.Select(m => m.SensorValue).OrderBy(v => v).ToList();
                Assert.That(sensor1Values[0], Is.EqualTo(22.5));
                Assert.That(sensor1Values[1], Is.EqualTo(23.0));

                // Verify Device 1 Sensor 2 measurements
                var sensor2Measurements = result.Measurements.FirstOrDefault(m => m.SensorIdentifier == device1Sensor2.Identifier);
                Assert.That(sensor2Measurements, Is.Not.Null, "Device 1 Sensor 2 should be in results");
                Assert.That(sensor2Measurements.Measurements.Count, Is.EqualTo(2), "Device 1 Sensor 2 should have 2 measurements");
                
                var sensor2Values = sensor2Measurements.Measurements.Select(m => m.SensorValue).OrderBy(v => v).ToList();
                Assert.That(sensor2Values[0], Is.EqualTo(65.0));
                Assert.That(sensor2Values[1], Is.EqualTo(67.5));

                // Verify Device 2 sensors are NOT in results
                var device2SensorMeasurements = result.Measurements.FirstOrDefault(m => m.SensorIdentifier == device2Sensor1.Identifier);
                Assert.That(device2SensorMeasurements, Is.Null, "Device 2 sensors should NOT be in results");
            });
        }
    }
}
