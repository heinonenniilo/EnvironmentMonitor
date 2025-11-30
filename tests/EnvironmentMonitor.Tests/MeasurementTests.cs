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

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Is.Not.Null);
            
            // Deserialize and verify the structure
            var result = JsonConvert.DeserializeObject<MeasurementsByLocationModel>(content);
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

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<MeasurementsByLocationModel>(content);
            
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
        }
    }
}
