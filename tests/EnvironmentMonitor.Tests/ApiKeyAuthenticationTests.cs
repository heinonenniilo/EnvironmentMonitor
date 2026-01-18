using EnvironmentMonitor.Application.DTOs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Assert = NUnit.Framework.Assert;

namespace EnvironmentMonitor.Tests
{
    [TestFixture]
    public class ApiKeyAuthenticationTests : BaseIntegrationTest
    {
        private string? _apiKey;

        [SetUp]
        public async Task Setup()
        {
            using var scope = _factory.Services.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            _apiKey = configuration.GetSection("ApiKey").Value;
            await LogoutAsync();
        }

        [Test]
        public async Task AddMeasurements_WithValidApiKey_ReturnsSuccess()
        {
            // Arrange
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;

            var measurementDto = new SaveMeasurementsDto
            {
                DeviceId = device.DeviceIdentifier,
                Measurements = new List<AddMeasurementDto>
                {
                    new AddMeasurementDto
                    {
                        SensorId = device.Sensors.First().SensorId,
                        SensorValue = 23.5,
                        TypeId = 1,
                        TimestampUtc = DateTime.UtcNow
                    }
                },
                FirstMessage = false
            };

            var json = JsonConvert.SerializeObject(measurementDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Add API Key header
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);

            // Act
            var response = await _client.PostAsync("/api/measurements", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task AddMeasurements_WithInvalidApiKey_ReturnsUnauthorized()
        {
            // Arrange
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;

            var measurementDto = new SaveMeasurementsDto
            {
                DeviceId = device.Identifier.ToString(),
                Measurements = new List<AddMeasurementDto>
                {
                    new AddMeasurementDto
                    {
                        SensorId = device.Sensors.First().SensorId,
                        SensorValue = 23.5,
                        TypeId = 1,
                        TimestampUtc = DateTime.UtcNow
                    }
                },
                FirstMessage = false
            };

            var json = JsonConvert.SerializeObject(measurementDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Add INVALID API Key header
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-API-KEY", "invalid-api-key-12345");

            // Act
            var response = await _client.PostAsync("/api/measurements", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task AddMeasurements_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;

            var measurementDto = new SaveMeasurementsDto
            {
                DeviceId = device.Identifier.ToString(),
                Measurements = new List<AddMeasurementDto>
                {
                    new AddMeasurementDto
                    {
                        SensorId = device.Sensors.First().SensorId,
                        SensorValue = 23.5,
                        TypeId = 1,
                        TimestampUtc = DateTime.UtcNow
                    }
                },
                FirstMessage = false
            };

            var json = JsonConvert.SerializeObject(measurementDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Don't add any authentication
            _client.DefaultRequestHeaders.Clear();

            // Act
            var response = await _client.PostAsync("/api/measurements", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task AddMeasurements_WithApiKey_AddsMultipleMeasurements()
        {
            // Arrange
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;
            var sensor1 = device.Sensors.First(s => s.Name == "Temperature-Sensor-01");
            var sensor2 = device.Sensors.First(s => s.Name == "Temperature-Sensor-02");

            var measurementDto = new SaveMeasurementsDto
            {
                DeviceId = device.DeviceIdentifier ,
                Measurements = new List<AddMeasurementDto>
                {
                    new AddMeasurementDto
                    {
                        SensorId = sensor1.SensorId,
                        SensorValue = 23.5,
                        TypeId = 1,
                        TimestampUtc = DateTime.UtcNow
                    },
                    new AddMeasurementDto
                    {
                        SensorId = sensor2.SensorId,
                        SensorValue = 65.0,
                        TypeId = 2,
                        TimestampUtc = DateTime.UtcNow
                    }
                },
                FirstMessage = false
            };

            var json = JsonConvert.SerializeObject(measurementDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);

            // Act
            var response = await _client.PostAsync("/api/measurements", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task AddMeasurements_WithFirstMessage_ProcessesCorrectly()
        {
            // Arrange
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;

            var measurementDto = new SaveMeasurementsDto
            {
                DeviceId = device.DeviceIdentifier,
                Measurements = new List<AddMeasurementDto>
                {
                    new AddMeasurementDto
                    {
                        SensorId = device.Sensors.First().SensorId,
                        SensorValue = 22.0,
                        TypeId = 1,
                        TimestampUtc = DateTime.UtcNow
                    }
                },
                FirstMessage = true,
                Identifier = $"msg-{Guid.NewGuid()}",
                SequenceNumber = 1,
                Uptime = 1000,
                MessageCount = 1,
                LoopCount = 1
            };

            var json = JsonConvert.SerializeObject(measurementDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);

            // Act
            var response = await _client.PostAsync("/api/measurements", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetMeasurements_WithApiKey_DoesNotWork()
        {
            // Arrange - API Key should NOT work for GET endpoints (they only accept cookie auth)
            var model = await PrepareDatabase();
            
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);

            var queryParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("From", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"))
            };
            var path = QueryHelpers.AddQueryString("/api/measurements", queryParams);

            // Act
            var response = await _client.GetAsync(path);

            // Assert - Should be Forbidden because GET endpoints don't accept API Key auth
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task GetMeasurementsBySensor_WithApiKey_DoesNotWork()
        {
            // Arrange
            var model = await PrepareDatabase();
            
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);

            var queryParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("From", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"))
            };
            var path = QueryHelpers.AddQueryString("/api/measurements/bysensor", queryParams);

            // Act
            var response = await _client.GetAsync(path);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task GetMeasurementsByLocation_WithApiKey_DoesNotWork()
        {
            // Arrange
            var model = await PrepareDatabase();
            
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);

            var queryParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("LocationIdentifiers", model.Location.Identifier.ToString()),
                new KeyValuePair<string, string>("From", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"))
            };
            var path = QueryHelpers.AddQueryString("/api/measurements/bylocation", queryParams);

            // Act
            var response = await _client.GetAsync(path);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task GetMeasurements_WithCookieAuth_Works()
        {
            // Arrange - Cookie auth should work for GET endpoints
            var model = await PrepareDatabase();
            
            await LoginAsync(AdminUserName, AdminPassword);

            var queryParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("From", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"))
            };
            var path = QueryHelpers.AddQueryString("/api/measurements", queryParams);

            // Act
            var response = await _client.GetAsync(path);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task AddMeasurements_WithEmptyApiKey_ReturnsUnauthorized()
        {
            // Arrange
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;

            var measurementDto = new SaveMeasurementsDto
            {
                DeviceId = device.Identifier.ToString(),
                Measurements = new List<AddMeasurementDto>
                {
                    new AddMeasurementDto
                    {
                        SensorId = device.Sensors.First().SensorId,
                        SensorValue = 23.5,
                        TypeId = 1,
                        TimestampUtc = DateTime.UtcNow
                    }
                },
                FirstMessage = false
            };

            var json = JsonConvert.SerializeObject(measurementDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Add empty API Key header
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-API-KEY", "");

            // Act
            var response = await _client.PostAsync("/api/measurements", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task AddMeasurements_WithInvalidDeviceId_IsNotSuccessWithCorrectApiKey()
        {
            // Arrange
            var model = await PrepareDatabase();

            var measurementDto = new SaveMeasurementsDto
            {
                DeviceId = Guid.NewGuid().ToString(), // Non-existent device
                Measurements = new List<AddMeasurementDto>
                {
                    new AddMeasurementDto
                    {
                        SensorId = 1,
                        SensorValue = 23.5,
                        TypeId = 1,
                        TimestampUtc = DateTime.UtcNow
                    }
                },
                FirstMessage = false
            };

            var json = JsonConvert.SerializeObject(measurementDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);

            // Act
            var response = await _client.PostAsync("/api/measurements", content);

            // Assert
            // The service logs a warning but returns OK (based on HubObserver pattern)
            // This matches the existing behavior where invalid data is logged but doesn't fail the request
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(false));
        }
    }
}
