using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
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
        private const string DeviceWriterUserName = "devicewriter@test.com";
        private const string DeviceWriterPassword = "DevWriter#123#4";
        private const string DeviceReadUserName = "deviceread@test.com";
        private const string DeviceReadPassword = "DevRead#123#4";

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

        [Test]
        public async Task DeviceWriterCanEditSensors()
        {
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;
            var sensor = device.Sensors.First();

            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                await userManager.CreateAsync(new ApplicationUser() { UserName = DeviceWriterUserName, Email = DeviceWriterUserName, EmailConfirmed = true }, DeviceWriterPassword);
                var user = await userManager.FindByEmailAsync(DeviceWriterUserName);
                await userManager.AddToRoleAsync(user, GlobalRoles.User.ToString());
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim(EntityRoles.DeviceWriter.ToString(), device.Identifier.ToString()));
            }

            await LoginAsync(DeviceWriterUserName, DeviceWriterPassword);

            // GET sensors should succeed
            var getResponse = await _client.GetAsync($"/api/sensors/{device.Identifier}");
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Add a sensor should succeed
            var addDto = new AddOrUpdateSensorDto
            {
                DeviceIdentifier = device.Identifier,
                SensorId = 55,
                Name = "Writer-Sensor",
                ScaleMin = 0,
                ScaleMax = 40,
                Active = true
            };
            var postContent = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            var postResponse = await _client.PostAsync("/api/sensors", postContent);
            var added = JsonConvert.DeserializeObject<SensorInfoDto>(await postResponse.Content.ReadAsStringAsync());

            Assert.Multiple(() =>
            {
                Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(added, Is.Not.Null);
                Assert.That(added.Name, Is.EqualTo("Writer-Sensor"));
                Assert.That(added.SensorId, Is.EqualTo(55));
            });

            // Update sensor should succeed
            var updateDto = new AddOrUpdateSensorDto
            {
                Identifier = added.Identifier,
                DeviceIdentifier = device.Identifier,
                Name = "Writer-Sensor-Updated",
                ScaleMin = 5,
                ScaleMax = 45,
                Active = false
            };
            var putContent = new StringContent(JsonConvert.SerializeObject(updateDto), Encoding.UTF8, "application/json");
            var putResponse = await _client.PutAsync("/api/sensors", putContent);
            var updated = JsonConvert.DeserializeObject<SensorInfoDto>(await putResponse.Content.ReadAsStringAsync());

            Assert.Multiple(() =>
            {
                Assert.That(putResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(updated.Name, Is.EqualTo("Writer-Sensor-Updated"));
                Assert.That(updated.Active, Is.False);
            });

            // Delete sensor should succeed
            var deleteResponse = await _client.DeleteAsync($"/api/sensors/{device.Identifier}/{added.Identifier}");
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task DeviceReadOnlyUserCannotEditSensors()
        {
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;
            var sensor = device.Sensors.First();

            // Create a user with User role and Device (read) claim — no DeviceWriter claim
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                await userManager.CreateAsync(new ApplicationUser() { UserName = DeviceReadUserName, Email = DeviceReadUserName, EmailConfirmed = true }, DeviceReadPassword);
                var user = await userManager.FindByEmailAsync(DeviceReadUserName);
                await userManager.AddToRoleAsync(user, GlobalRoles.User.ToString());
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim(EntityRoles.Device.ToString(), device.Identifier.ToString()));
            }

            await LoginAsync(DeviceReadUserName, DeviceReadPassword);

            // POST should fail at service level (UnauthorizedAccessException -> 500 or similar)
            var addDto = new AddOrUpdateSensorDto
            {
                DeviceIdentifier = device.Identifier,
                SensorId = 66,
                Name = "Should-Fail",
            };
            var postContent = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            var postResponse = await _client.PostAsync("/api/sensors", postContent);
            Assert.That(postResponse.IsSuccessStatusCode, Is.False);

            // PUT should fail
            var updateDto = new AddOrUpdateSensorDto
            {
                Identifier = sensor.Identifier,
                DeviceIdentifier = device.Identifier,
                Name = "Should-Fail",
            };
            var putContent = new StringContent(JsonConvert.SerializeObject(updateDto), Encoding.UTF8, "application/json");
            var putResponse = await _client.PutAsync("/api/sensors", putContent);
            Assert.That(putResponse.IsSuccessStatusCode, Is.False);

            // DELETE should fail
            var deleteResponse = await _client.DeleteAsync($"/api/sensors/{device.Identifier}/{sensor.Identifier}");
            Assert.That(deleteResponse.IsSuccessStatusCode, Is.False);
        }

        [Test]
        public async Task ViewerCannotEditSensors()
        {
            var model = await PrepareDatabase();
            var device = model.DeviceInLocation;
            var sensor = device.Sensors.First();

            await LoginAsync(ViewerUserName, ViewerPassword);

            // POST should fail
            var addDto = new AddOrUpdateSensorDto
            {
                DeviceIdentifier = device.Identifier,
                SensorId = 88,
                Name = "Should-Fail",
            };
            var postContent = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            var postResponse = await _client.PostAsync("/api/sensors", postContent);
            Assert.That(postResponse.IsSuccessStatusCode, Is.False);

            // PUT should fail
            var updateDto = new AddOrUpdateSensorDto
            {
                Identifier = sensor.Identifier,
                DeviceIdentifier = device.Identifier,
                Name = "Should-Fail",
            };
            var putContent = new StringContent(JsonConvert.SerializeObject(updateDto), Encoding.UTF8, "application/json");
            var putResponse = await _client.PutAsync("/api/sensors", putContent);
            Assert.That(putResponse.IsSuccessStatusCode, Is.False);

            // DELETE should fail
            var deleteResponse = await _client.DeleteAsync($"/api/sensors/{device.Identifier}/{sensor.Identifier}");
            Assert.That(deleteResponse.IsSuccessStatusCode, Is.False);
        }
    }
}
