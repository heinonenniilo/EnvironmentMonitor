using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Identity;
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
    public class LocationManagementTests : BaseIntegrationTest
    {
        [Test]
        public async Task AdminCanAddLocation()
        {
            await PrepareDatabase();
            await LoginAsync(AdminUserName, AdminPassword);

            var addDto = new AddLocationDto { Name = "New-Location" };
            var content = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/locations", content);
            var result = JsonConvert.DeserializeObject<LocationDto>(await response.Content.ReadAsStringAsync());

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();
            var locationInDb = await db.Locations.FirstOrDefaultAsync(x => x.Identifier == result.Identifier);

            Assert.Multiple(() =>
            {
                Assert.That(locationInDb, Is.Not.Null);
                Assert.That(locationInDb.Name, Is.EqualTo("New-Location"));
                Assert.That(locationInDb.Visible, Is.True);
            });
        }

        [Test]
        public async Task AdminCanDeleteLocation()
        {
            await PrepareDatabase();
            await LoginAsync(AdminUserName, AdminPassword);

            var addDto = new AddLocationDto { Name = "To-Delete" };
            var content = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            var addResponse = await _client.PostAsync("/api/locations", content);
            var added = JsonConvert.DeserializeObject<LocationDto>(await addResponse.Content.ReadAsStringAsync());

            var deleteResponse = await _client.DeleteAsync($"/api/locations/{added.Identifier}");
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();
            var locationInDb = await db.Locations.FirstOrDefaultAsync(x => x.Identifier == added.Identifier);

            Assert.That(locationInDb, Is.Null);
        }

        [Test]
        public async Task AdminCanUpdateLocation()
        {
            await PrepareDatabase();
            await LoginAsync(AdminUserName, AdminPassword);

            var addDto = new AddLocationDto { Name = "Before-Update" };
            var content = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            var addResponse = await _client.PostAsync("/api/locations", content);
            var added = JsonConvert.DeserializeObject<LocationDto>(await addResponse.Content.ReadAsStringAsync());

            added.Name = "After-Update";
            added.Visible = false;
            var updateContent = new StringContent(JsonConvert.SerializeObject(added), Encoding.UTF8, "application/json");
            var updateResponse = await _client.PutAsync("/api/locations", updateContent);

            Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();
            var locationInDb = await db.Locations.FirstOrDefaultAsync(x => x.Identifier == added.Identifier);

            Assert.Multiple(() =>
            {
                Assert.That(locationInDb, Is.Not.Null);
                Assert.That(locationInDb.Name, Is.EqualTo("After-Update"));
                Assert.That(locationInDb.Visible, Is.False);
            });
        }

        [Test]
        public async Task AdminCanAddLocationSensor()
        {
            var model = await PrepareDatabase();
            await LoginAsync(AdminUserName, AdminPassword);

            var sensor = model.DeviceInLocation.Sensors.First();
            var addDto = new AddOrUpdateLocationSensorDto
            {
                LocationIdentifier = model.Location.Identifier,
                SensorIdentifier = sensor.Identifier,
                Name = "Location-Sensor-1",
                TypeId = 1
            };

            var content = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/locations/sensors", content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();
            var lsInDb = await db.LocationSensors
                .FirstOrDefaultAsync(x => x.LocationId == model.Location.Id && x.SensorId == sensor.Id);

            Assert.Multiple(() =>
            {
                Assert.That(lsInDb, Is.Not.Null);
                Assert.That(lsInDb.Name, Is.EqualTo("Location-Sensor-1"));
                Assert.That(lsInDb.TypeId, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task AdminCanUpdateLocationSensor()
        {
            var model = await PrepareDatabase();
            await LoginAsync(AdminUserName, AdminPassword);

            var sensor = model.DeviceInLocation.Sensors.First();

            // Add first
            var addDto = new AddOrUpdateLocationSensorDto
            {
                LocationIdentifier = model.Location.Identifier,
                SensorIdentifier = sensor.Identifier,
                Name = "Original-Name",
                TypeId = 1
            };
            var addContent = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/locations/sensors", addContent);

            // Update
            var updateDto = new AddOrUpdateLocationSensorDto
            {
                LocationIdentifier = model.Location.Identifier,
                SensorIdentifier = sensor.Identifier,
                Name = "Updated-Name",
                TypeId = 2
            };
            var updateContent = new StringContent(JsonConvert.SerializeObject(updateDto), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync("/api/locations/sensors", updateContent);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();
            var lsInDb = await db.LocationSensors
                .FirstOrDefaultAsync(x => x.LocationId == model.Location.Id && x.SensorId == sensor.Id);

            Assert.Multiple(() =>
            {
                Assert.That(lsInDb, Is.Not.Null);
                Assert.That(lsInDb.Name, Is.EqualTo("Updated-Name"));
                Assert.That(lsInDb.TypeId, Is.EqualTo(2));
            });
        }

        [Test]
        public async Task AdminCanDeleteLocationSensor()
        {
            var model = await PrepareDatabase();
            await LoginAsync(AdminUserName, AdminPassword);

            var sensor = model.DeviceInLocation.Sensors.First();

            // Add first
            var addDto = new AddOrUpdateLocationSensorDto
            {
                LocationIdentifier = model.Location.Identifier,
                SensorIdentifier = sensor.Identifier,
                Name = "To-Remove",
            };
            var addContent = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/locations/sensors", addContent);

            // Delete
            var deleteResponse = await _client.DeleteAsync($"/api/locations/{model.Location.Identifier}/sensors/{sensor.Identifier}");

            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();
            var lsInDb = await db.LocationSensors
                .FirstOrDefaultAsync(x => x.LocationId == model.Location.Id && x.SensorId == sensor.Id);

            Assert.That(lsInDb, Is.Null);
        }

        [Test]
        public async Task AdminCanMoveDevicesToLocation()
        {
            var model = await PrepareDatabase();
            await LoginAsync(AdminUserName, AdminPassword);

            // Add a new location to move a device to
            var addDto = new AddLocationDto { Name = "Target-Location" };
            var addContent = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            var addResponse = await _client.PostAsync("/api/locations", addContent);
            var targetLocation = JsonConvert.DeserializeObject<LocationDto>(await addResponse.Content.ReadAsStringAsync());

            var moveDto = new MoveDevicesToLocationDto
            {
                LocationIdentifier = targetLocation.Identifier,
                DeviceIdentifiers = [model.DeviceInLocation.Identifier]
            };
            var moveContent = new StringContent(JsonConvert.SerializeObject(moveDto), Encoding.UTF8, "application/json");
            var moveResponse = await _client.PostAsync("/api/locations/move-devices", moveContent);

            Assert.That(moveResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();
            var deviceInDb = await db.Devices.FirstOrDefaultAsync(x => x.Identifier == model.DeviceInLocation.Identifier);
            var targetLocationInDb = await db.Locations.FirstOrDefaultAsync(x => x.Identifier == targetLocation.Identifier);

            Assert.Multiple(() =>
            {
                Assert.That(deviceInDb, Is.Not.Null);
                Assert.That(targetLocationInDb, Is.Not.Null);
                Assert.That(deviceInDb.LocationId, Is.EqualTo(targetLocationInDb.Id));
            });
        }

        [Test]
        public async Task ViewerCannotAddLocation()
        {
            await PrepareDatabase();
            await LoginAsync(ViewerUserName, ViewerPassword);

            var addDto = new AddLocationDto { Name = "Should-Fail" };
            var content = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/locations", content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task ViewerCannotDeleteLocation()
        {
            var model = await PrepareDatabase();
            await LoginAsync(ViewerUserName, ViewerPassword);

            var response = await _client.DeleteAsync($"/api/locations/{model.Location.Identifier}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task ViewerCannotUpdateLocation()
        {
            var model = await PrepareDatabase();
            await LoginAsync(ViewerUserName, ViewerPassword);

            var updateDto = new LocationDto
            {
                Identifier = model.Location.Identifier,
                Name = "Should-Fail",
                Visible = false
            };
            var content = new StringContent(JsonConvert.SerializeObject(updateDto), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync("/api/locations", content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task ViewerCannotAddLocationSensor()
        {
            var model = await PrepareDatabase();
            await LoginAsync(ViewerUserName, ViewerPassword);

            var sensor = model.DeviceInLocation.Sensors.First();
            var addDto = new AddOrUpdateLocationSensorDto
            {
                LocationIdentifier = model.Location.Identifier,
                SensorIdentifier = sensor.Identifier,
                Name = "Should-Fail",
            };
            var content = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/locations/sensors", content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task ViewerCannotDeleteLocationSensor()
        {
            var model = await PrepareDatabase();
            await LoginAsync(ViewerUserName, ViewerPassword);

            var sensor = model.DeviceInLocation.Sensors.First();
            var response = await _client.DeleteAsync($"/api/locations/{model.Location.Identifier}/sensors/{sensor.Identifier}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task ViewerCannotMoveDevices()
        {
            var model = await PrepareDatabase();
            await LoginAsync(ViewerUserName, ViewerPassword);

            var moveDto = new MoveDevicesToLocationDto
            {
                LocationIdentifier = model.Location.Identifier,
                DeviceIdentifiers = [model.DeviceUnssigned.Identifier]
            };
            var content = new StringContent(JsonConvert.SerializeObject(moveDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/locations/move-devices", content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task BasicUserCannotEditLocations()
        {
            var model = await PrepareDatabase();
            await LoginAsync(LocationUserName, LocationUserPassword);

            // Cannot add location
            var addDto = new AddLocationDto { Name = "Should-Fail" };
            var addContent = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
            var addResponse = await _client.PostAsync("/api/locations", addContent);

            // Cannot delete location
            var deleteResponse = await _client.DeleteAsync($"/api/locations/{model.Location.Identifier}");

            // Cannot update location
            var updateDto = new LocationDto
            {
                Identifier = model.Location.Identifier,
                Name = "Should-Fail",
                Visible = false
            };
            var updateContent = new StringContent(JsonConvert.SerializeObject(updateDto), Encoding.UTF8, "application/json");
            var updateResponse = await _client.PutAsync("/api/locations", updateContent);

            // Cannot add location sensor
            var sensor = model.DeviceInLocation.Sensors.First();
            var sensorDto = new AddOrUpdateLocationSensorDto
            {
                LocationIdentifier = model.Location.Identifier,
                SensorIdentifier = sensor.Identifier,
                Name = "Should-Fail",
            };
            var sensorContent = new StringContent(JsonConvert.SerializeObject(sensorDto), Encoding.UTF8, "application/json");
            var sensorResponse = await _client.PostAsync("/api/locations/sensors", sensorContent);

            // Cannot move devices
            var moveDto = new MoveDevicesToLocationDto
            {
                LocationIdentifier = model.Location.Identifier,
                DeviceIdentifiers = [model.DeviceUnssigned.Identifier]
            };
            var moveContent = new StringContent(JsonConvert.SerializeObject(moveDto), Encoding.UTF8, "application/json");
            var moveResponse = await _client.PostAsync("/api/locations/move-devices", moveContent);

            Assert.Multiple(() =>
            {
                Assert.That(addResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                Assert.That(sensorResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                Assert.That(moveResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            });
        }

        [Test]
        public async Task UserCanFetchAccessibleLocations()
        {
            var model = await PrepareDatabase();

            // Add a third location and give LocationUser access to it
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();

                var extraLocation = new Location { Name = "Extra-Location" };
                db.Locations.Add(extraLocation);
                await db.SaveChangesAsync();

                var user = await userManager.FindByEmailAsync(LocationUserName);
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim(
                   EntityRoles.Location.ToString(), extraLocation.Identifier.ToString()));
            }

            await LoginAsync(LocationUserName, LocationUserPassword);

            var response = await _client.GetAsync("/api/locations");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var locations = JsonConvert.DeserializeObject<List<LocationDto>>(await response.Content.ReadAsStringAsync());

            Assert.Multiple(() =>
            {
                Assert.That(locations, Has.Count.EqualTo(2));
                Assert.That(locations.Any(l => l.Identifier == model.Location.Identifier), Is.True);
                Assert.That(locations.Any(l => l.Identifier == model.LocationWithNoDefinedAccess.Identifier), Is.False);
            });
        }

        [Test]
        public async Task UserCanFetchSingleAccessibleLocation()
        {
            var model = await PrepareDatabase();
            await LoginAsync(LocationUserName, LocationUserPassword);

            var response = await _client.GetAsync($"/api/locations/{model.Location.Identifier}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var location = JsonConvert.DeserializeObject<LocationDto>(await response.Content.ReadAsStringAsync());

            Assert.Multiple(() =>
            {
                Assert.That(location, Is.Not.Null);
                Assert.That(location.Identifier, Is.EqualTo(model.Location.Identifier));
                Assert.That(location.LocationSensors, Is.Not.Null);
            });
        }

        [Test]
        public async Task UserCannotFetchLocationWithNoAccess()
        {
            var model = await PrepareDatabase();
            await LoginAsync(LocationUserName, LocationUserPassword);

            var response = await _client.GetAsync($"/api/locations/{model.LocationWithNoDefinedAccess.Identifier}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task AdminCanFetchAllLocations()
        {
            var model = await PrepareDatabase();
            await LoginAsync(AdminUserName, AdminPassword);

            var response = await _client.GetAsync("/api/locations");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var locations = JsonConvert.DeserializeObject<List<LocationDto>>(await response.Content.ReadAsStringAsync());

            Assert.Multiple(() =>
            {
                Assert.That(locations.Any(l => l.Identifier == model.Location.Identifier), Is.True);
                Assert.That(locations.Any(l => l.Identifier == model.LocationWithNoDefinedAccess.Identifier), Is.True);
            });
        }

        [Test]
        public async Task AdminCanFetchSingleLocationWithDevices()
        {
            var model = await PrepareDatabase();
            await LoginAsync(AdminUserName, AdminPassword);

            var response = await _client.GetAsync($"/api/locations/{model.Location.Identifier}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var location = JsonConvert.DeserializeObject<LocationDto>(await response.Content.ReadAsStringAsync());

            Assert.Multiple(() =>
            {
                Assert.That(location, Is.Not.Null);
                Assert.That(location.Identifier, Is.EqualTo(model.Location.Identifier));
                Assert.That(location.Devices, Is.Not.Null);
                Assert.That(location.Devices.Count, Is.GreaterThan(0));
                Assert.That(location.LocationSensors, Is.Not.Null);
            });
        }
    }
}
