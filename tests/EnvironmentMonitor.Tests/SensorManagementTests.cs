using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;
using System.Text;
using Assert = NUnit.Framework.Assert;

namespace EnvironmentMonitor.Tests
{
    [TestFixture]
    public class SensorManagementTests : BaseIntegrationTest
    {
        [Test]
        public async Task OnlyAdminCanAccessSensorsController()
        {
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;

            // Viewer should get Forbidden
            await LoginAsync(ViewerUserName, ViewerPassword);
            var viewerResponse = await _client.GetAsync($"/api/sensors/{device.Identifier}");
            Assert.That(viewerResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

            // Regular user should get Forbidden
            await LogoutAsync();
            await LoginAsync(LocationUserName, LocationUserPassword);
            var userResponse = await _client.GetAsync($"/api/sensors/{device.Identifier}");
            Assert.That(userResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

            // Admin should succeed
            await LogoutAsync();
            await LoginAsync(AdminUserName, AdminPassword);
            var adminResponse = await _client.GetAsync($"/api/sensors/{device.Identifier}");
            Assert.That(adminResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task AdminCanAddSensor()
        {
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;
            await LoginAsync(AdminUserName, AdminPassword);

            var addDto = new AddOrUpdateSensorDto
            {
                DeviceIdentifier = device.Identifier,
                SensorId = 99,
                Name = "New-Sensor",
                ScaleMin = -10,
                ScaleMax = 50,
                Active = true
            };

            var content = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/sensors", content);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SensorInfoDto>(body);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Name, Is.EqualTo("New-Sensor"));
                Assert.That(result.SensorId, Is.EqualTo(99));
                Assert.That(result.ScaleMin, Is.EqualTo(-10));
                Assert.That(result.ScaleMax, Is.EqualTo(50));
                Assert.That(result.Active, Is.True);
                Assert.That(result.Identifier, Is.Not.EqualTo(Guid.Empty));
            });

            // Verify it shows up in GET
            var getResponse = await _client.GetAsync($"/api/sensors/{device.Identifier}");
            var sensors = JsonConvert.DeserializeObject<List<SensorInfoDto>>(await getResponse.Content.ReadAsStringAsync());
            Assert.That(sensors.Any(s => s.SensorId == 99 && s.Name == "New-Sensor"), Is.True);
        }

        [Test]
        public async Task AdminCanUpdateSensor()
        {
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;
            var sensor = device.Sensors.First();
            await LoginAsync(AdminUserName, AdminPassword);

            var updateDto = new AddOrUpdateSensorDto
            {
                Identifier = sensor.Identifier,
                DeviceIdentifier = device.Identifier,
                Name = "Updated-Name",
                ScaleMin = -5,
                ScaleMax = 100,
                Active = false
            };

            var content = new StringContent(JsonConvert.SerializeObject(updateDto), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync("/api/sensors", content);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SensorInfoDto>(body);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Name, Is.EqualTo("Updated-Name"));
                Assert.That(result.ScaleMin, Is.EqualTo(-5));
                Assert.That(result.ScaleMax, Is.EqualTo(100));
                Assert.That(result.Active, Is.False);
                Assert.That(result.Identifier, Is.EqualTo(sensor.Identifier));
            });
        }

        [Test]
        public async Task AdminCanDeleteSensor()
        {
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;
            await LoginAsync(AdminUserName, AdminPassword);

            // Add a sensor with no measurements so it can be deleted
            var addDto = new AddOrUpdateSensorDto
            {
                DeviceIdentifier = device.Identifier,
                SensorId = 77,
                Name = "To-Delete",
            };
            var addContent = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            var addResponse = await _client.PostAsync("/api/sensors", addContent);
            var added = JsonConvert.DeserializeObject<SensorInfoDto>(await addResponse.Content.ReadAsStringAsync());

            // Delete it
            var deleteResponse = await _client.DeleteAsync($"/api/sensors/{device.Identifier}/{added.Identifier}");
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Verify it's gone
            var getResponse = await _client.GetAsync($"/api/sensors/{device.Identifier}");
            var sensors = JsonConvert.DeserializeObject<List<SensorInfoDto>>(await getResponse.Content.ReadAsStringAsync());
            Assert.That(sensors.Any(s => s.Identifier == added.Identifier), Is.False);
        }

        [Test]
        public async Task ViewerCannotAddSensor()
        {
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;
            await LoginAsync(ViewerUserName, ViewerPassword);

            var addDto = new AddOrUpdateSensorDto
            {
                DeviceIdentifier = device.Identifier,
                SensorId = 88,
                Name = "Should-Fail",
            };

            var content = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/sensors", content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task ViewerCannotUpdateSensor()
        {
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;
            var sensor = device.Sensors.First();
            await LoginAsync(ViewerUserName, ViewerPassword);

            var updateDto = new AddOrUpdateSensorDto
            {
                Identifier = sensor.Identifier,
                DeviceIdentifier = device.Identifier,
                Name = "Should-Fail",
            };

            var content = new StringContent(JsonConvert.SerializeObject(updateDto), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync("/api/sensors", content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task ViewerCannotDeleteSensor()
        {
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;
            var sensor = device.Sensors.First();
            await LoginAsync(ViewerUserName, ViewerPassword);

            var response = await _client.DeleteAsync($"/api/sensors/{device.Identifier}/{sensor.Identifier}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task GetSensorsReturnsAllSensorsForDevice()
        {
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;
            await LoginAsync(AdminUserName, AdminPassword);

            var response = await _client.GetAsync($"/api/sensors/{device.Identifier}");
            var body = await response.Content.ReadAsStringAsync();
            var sensors = JsonConvert.DeserializeObject<List<SensorInfoDto>>(body);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(sensors, Is.Not.Null);
                Assert.That(sensors.Count, Is.EqualTo(device.Sensors.Count));
            });
        }
    }
}
