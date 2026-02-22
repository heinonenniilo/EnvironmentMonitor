using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Infrastructure.Data;
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

        private async Task<(string SecretId, string SecretValue)> CreateApiKeyAsync(List<Guid> deviceIds, List<Guid>? locationIds = null, string? description = null)
        {
            // Login as admin to create API key
            await LoginAsync(AdminUserName, AdminPassword);

            // Create API key with access to the specified devices/locations
            var createApiKeyRequest = new
            {
                DeviceIds = deviceIds,
                LocationIds = locationIds ?? new List<Guid>(),
                Description = description ?? "Test API Key"
            };

            var createKeyJson = JsonConvert.SerializeObject(createApiKeyRequest);
            var createKeyContent = new StringContent(createKeyJson, Encoding.UTF8, "application/json");
            var createKeyResponse = await _client.PostAsync("/api/apikeys", createKeyContent);
            
            Assert.That(createKeyResponse.IsSuccessStatusCode, Is.True, 
                $"Failed to create API key. Status: {createKeyResponse.StatusCode}, Response: {await createKeyResponse.Content.ReadAsStringAsync()}");
            
            var createKeyResponseJson = await createKeyResponse.Content.ReadAsStringAsync();
            var apiKeyResponse = JsonConvert.DeserializeObject<dynamic>(createKeyResponseJson);
            
            string secretValue = apiKeyResponse.apiKey;
            string secretId = apiKeyResponse.id;

            // Logout after creating the API key
            await LogoutAsync();

            return (secretId, secretValue);
        }

        [Test]
        public async Task AddMeasurements_WithValidApiKey_ReturnsSuccess()
        {
            // Arrange
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;
            var (secretId, secretValue) = await CreateApiKeyAsync(
                deviceIds: new List<Guid> { device.Identifier },
                description: "Test API Key for Device Access"
            );

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
            _client.DefaultRequestHeaders.Add("X-SECRET-ID", secretId);
            _client.DefaultRequestHeaders.Add("X-SECRET-VALUE", secretValue);

            // Act
            var response = await _client.PostAsync("/api/measurements", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task AddMeasurements_NonAccessibleDevice_Returnsfalse()
        {
            // Arrange
            var model = await PrepareDatabase();
            var apiKeyDevice = model.DeviceInLocation;
            var measurementDevic = model.DeviceInLocationWithNoAccess;

            var (secretId, secretValue) = await CreateApiKeyAsync(
                deviceIds: new List<Guid> { apiKeyDevice.Identifier },
                description: "Test API Key for Device Access"
            );

            var measurementDto = new SaveMeasurementsDto
            {
                DeviceId = measurementDevic.DeviceIdentifier,
                Measurements = new List<AddMeasurementDto>
                {
                    new AddMeasurementDto
                    {
                        SensorId = apiKeyDevice.Sensors.First().SensorId,
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
            _client.DefaultRequestHeaders.Add("X-SECRET-ID", secretId);
            _client.DefaultRequestHeaders.Add("X-SECRET-VALUE", secretValue);

            // Act
            var response = await _client.PostAsync("/api/measurements", content);

            // Assert
            Assert.That(response.IsSuccessStatusCode, Is.False);
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

            var (secretId, secretValue) = await CreateApiKeyAsync(
                deviceIds: new List<Guid> { device.Identifier },
                description: "Test API Key for Device Access"
            );

            // Prepare measurement request
            var measurementDto = new SaveMeasurementsDto
            {
                DeviceId = device.DeviceIdentifier,
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
            _client.DefaultRequestHeaders.Add("X-SECRET-ID", secretId); 
            _client.DefaultRequestHeaders.Add("X-SECRET-VALUE", secretValue); 

            // Act
            var response = await _client.PostAsync("/api/measurements", content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), 
                $"Expected OK but got {response.StatusCode}. Response: {await response.Content.ReadAsStringAsync()}");
        }

        [Test]
        public async Task AddMeasurements_WithFirstMessage_ProcessesCorrectly()
        {
            // Arrange
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;
            var (secretId, secretValue) = await CreateApiKeyAsync(
                deviceIds: new List<Guid> { device.Identifier },
                description: "Test API Key for Device Access"
            );

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
            _client.DefaultRequestHeaders.Add("X-SECRET-ID", secretId); // Secret ID from database
            _client.DefaultRequestHeaders.Add("X-SECRET-VALUE", secretValue); // Plain secret value

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

        [Test]
        public async Task GetDeviceAttributes_WithApiKey_ReturnsSuccess()
        {
            // Arrange
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;

            // Add device attributes using the database context
            using (var scope = _factory.Services.CreateScope())
            {
                var measurementDbContext = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();
                var dateService = scope.ServiceProvider.GetRequiredService<IDateService>();
                
                var currentTime = dateService.CurrentTime();
                var currentTimeUtc = dateService.LocalToUtc(currentTime);
                
                // Add MotionControlStatus attribute (TypeId = 0, value = 1 for AlwaysOn)
                measurementDbContext.DeviceAttributes.Add(new DeviceAttribute
                {
                    DeviceId = device.Id,
                    TypeId = (int)DeviceAttributeTypes.MotionControlStatus,
                    Value = "1",
                    TimeStamp = currentTime,
                    TimeStampUtc = currentTimeUtc,
                    Created = currentTime,
                    CreatedUtc = currentTimeUtc
                });
                
                // Add OnDelay attribute (TypeId = 1, value = 5000ms)
                measurementDbContext.DeviceAttributes.Add(new DeviceAttribute
                {
                    DeviceId = device.Id,
                    TypeId = (int)DeviceAttributeTypes.OnDelay,
                    Value = "5000",
                    TimeStamp = currentTime,
                    TimeStampUtc = currentTimeUtc,
                    Created = currentTime,
                    CreatedUtc = currentTimeUtc
                });
                
                await measurementDbContext.SaveChangesAsync();
            }

            // Create API key with access to the device
            var (secretId, secretValue) = await CreateApiKeyAsync(
                deviceIds: new List<Guid> { device.Identifier },
                description: "Test API Key for Device Attributes Access"
            );

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);
            _client.DefaultRequestHeaders.Add("X-SECRET-ID", secretId);
            _client.DefaultRequestHeaders.Add("X-SECRET-VALUE", secretValue);

            // Act
            var response = await _client.GetAsync($"/api/devicecommands/{device.DeviceIdentifier}/attributes");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                $"Expected OK but got {response.StatusCode}. Response: {await response.Content.ReadAsStringAsync()}");
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var attributes = JsonConvert.DeserializeObject<Dictionary<int, string>>(responseContent);
            
            Assert.That(attributes, Is.Not.Null, "Attributes dictionary should not be null");
            Assert.That(attributes.Count, Is.EqualTo(2), "Should have 2 attributes");
            Assert.That(attributes.ContainsKey((int)DeviceAttributeTypes.MotionControlStatus), Is.True, 
                "Should contain MotionControlStatus attribute");
            Assert.That(attributes[(int)DeviceAttributeTypes.MotionControlStatus], Is.EqualTo("1"), 
                "MotionControlStatus should be '1'");
            Assert.That(attributes.ContainsKey((int)DeviceAttributeTypes.OnDelay), Is.True, 
                "Should contain OnDelay attribute");
            Assert.That(attributes[(int)DeviceAttributeTypes.OnDelay], Is.EqualTo("5000"), 
                "OnDelay should be '5000'");
        }

        [Test]
        public async Task GetDeviceAttributes_WithCookieAuth_ReturnsForbidden()
        {
            // Arrange
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;

            // Login with cookie auth
            await LoginAsync(AdminUserName, AdminPassword);

            // Act
            var response = await _client.GetAsync($"/api/devicecommands/{device.DeviceIdentifier}/attributes");

            // Assert - Cookie auth should NOT work for this endpoint, only API Key auth is allowed
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized),
                $"Expected Unauthorized but got {response.StatusCode}. Response: {await response.Content.ReadAsStringAsync()}");
        }
    }
}
