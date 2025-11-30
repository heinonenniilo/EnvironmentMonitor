using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Extensions;
using EnvironmentMonitor.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using Respawn;
using Respawn.Graph;
using System.Text;
using Device = EnvironmentMonitor.Domain.Entities.Device;

namespace EnvironmentMonitor.Tests
{
    public abstract class BaseIntegrationTest
    {
        protected CustomWebApplicationFactory<Program> _factory;
        protected HttpClient _client;
        protected const string AdminPassword = "adminPassword#1#55";
        protected const string AdminUserName = "Admin@admin.com";
        protected const string ViewerPassword = "viewerPw#09#22";
        protected const string ViewerUserName = "Viewer@Viewer.com";
        protected const string LocationUserName = "basic@basic.fi";
        protected const string LocationUserPassword = "basicPw#09#09";
        protected const string DeviceUserName = "deviceuser@device.com";
        protected const string DeviceUserPassword = "BasicPw#0123#2";

        protected Respawner? _respawner;
        protected IConfigurationRoot _configuration;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.testing.json")
                .AddEnvironmentVariables()
                .Build();
            var services = new ServiceCollection();
            services.AddInfrastructureServices(_configuration);
            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var measureDbContext = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();
                var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                measureDbContext.Database.EnsureDeleted();
                applicationDbContext.Database.EnsureDeleted();
                measureDbContext.Database.Migrate();
                applicationDbContext.Database.Migrate();
            }
            _factory = new CustomWebApplicationFactory<Program>();
            _respawner = await Respawner.CreateAsync(_configuration.GetConnectionString("DefaultConnection"), new RespawnerOptions
            {
                TablesToIgnore =
                [
                    new Table("dbo", "MeasurementTypes"),
                    new Table("dbo", "Locations"),
                    new Table("application", "AspNetRoles")
                ],
            });
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory.Dispose();
        }

        [TearDown]
        public async Task Clean()
        {
            if (_respawner != null)
            {
                await _respawner.ResetAsync(_configuration.GetConnectionString("DefaultConnection"));
                using (var scope = _factory.Services.CreateScope())
                {
                    var measurementDbContext = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();
                    measurementDbContext.Locations.RemoveRange(measurementDbContext.Locations.Where(x => x.Id != 0));
                    measurementDbContext.SaveChanges();
                }
            }
        }

        protected async Task<bool> LoginAsync(string email, string password)
        {
            var loginData = new LoginModel()
            {
                UserName = email,
                Password = password,
                Persistent = true
            };

            var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/Authentication/login", content);
            return loginResponse.IsSuccessStatusCode;
        }

        protected async Task AddLocationSensorsAndMeasurements(
            int locationId,
            int sensorId,
            int deviceId,
            string locationSensorName,
            int typeId,
            List<(double value, DateTime timestamp)> measurements)
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var measurementDbContext = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();

                // Re-query entities in this scope to get tracked instances
                var location = await measurementDbContext.Locations.FindAsync(locationId);
                var sensor = await measurementDbContext.Sensors.FindAsync(sensorId);
                var device = await measurementDbContext.Devices.FindAsync(deviceId);

                // Add LocationSensor
                var locationSensor = new LocationSensor
                {
                    Location = location,
                    Sensor = sensor,
                    Device = device,
                    Name = locationSensorName,
                    TypeId = typeId
                };
                measurementDbContext.LocationSensors.Add(locationSensor);
                await measurementDbContext.SaveChangesAsync();

                // Add measurements
                foreach (var (value, timestamp) in measurements)
                {
                    measurementDbContext.Measurements.Add(new Measurement
                    {
                        SensorId = sensorId,
                        TypeId = typeId,
                        Value = value,
                        Timestamp = timestamp,
                        TimestampUtc = timestamp.ToUniversalTime(),
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow
                    });
                }

                await measurementDbContext.SaveChangesAsync();
            }
        }

        protected async Task<PrepareDbModel> PrepareDatabase()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var measurementDbContext = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();
                var adminUser = await userManager.CreateAsync(new ApplicationUser() { UserName = AdminUserName, Email = AdminUserName }, AdminPassword);
                var viewerUser = await userManager.CreateAsync(new ApplicationUser() { UserName = ViewerUserName, Email = ViewerUserName }, ViewerPassword);
                var basicUser = await userManager.CreateAsync(new ApplicationUser() { UserName = LocationUserName, Email = LocationUserName }, LocationUserPassword);
                var deviceUser = await userManager.CreateAsync(new ApplicationUser() { UserName = DeviceUserName, Email = DeviceUserName }, DeviceUserPassword);

                var location = new Location()
                {
                    Name = "Test",
                };

                var locationWithNoAccess = new Location()
                {
                    Name = "Another"
                };
                measurementDbContext.Locations.Add(location);
                measurementDbContext.Locations.Add(locationWithNoAccess);
                measurementDbContext.SaveChanges();
                var deviceInLocation = new Device()
                {
                    Name = "InLocation",
                    DeviceIdentifier = "DEVICE-01",
                    Sensors = [
                        new Sensor() {
                            Name = "Temperature-Sensor-01",
                            SensorId = 1
                        },
                        new Sensor() {
                            Name = "Temperature-Sensor-02",
                            SensorId = 2
                        }
                    ],
                    Location = location
                };

                var deviceWithBaseLocation = new Device()
                {
                    Name = "Device",
                    DeviceIdentifier = "Device-02",
                    Sensors = [new Sensor()
                    {
                        Name = "Test-01",
                        SensorId = 1
                    }],
                    Location = measurementDbContext.Locations.First(x => x.Id == 0)
                };


                var deviceInLocationWithNoAccess = new Device()
                {
                    Name = "AnotherDevivce",
                    DeviceIdentifier = "Device-03",
                    Sensors = [new Sensor() {
                        Name = "Test-01",
                        SensorId = 1
                    }],
                    Location = locationWithNoAccess
                };
                measurementDbContext.Devices.Add(deviceWithBaseLocation);
                measurementDbContext.Devices.Add(deviceInLocation);
                measurementDbContext.Devices.Add(deviceInLocationWithNoAccess);
                measurementDbContext.SaveChanges();


                await userManager.AddToRoleAsync(await userManager.FindByEmailAsync(AdminUserName), GlobalRoles.Admin.ToString());
                await userManager.AddToRoleAsync(await userManager.FindByEmailAsync(AdminUserName), GlobalRoles.User.ToString());

                await userManager.AddToRoleAsync(await userManager.FindByEmailAsync(ViewerUserName), GlobalRoles.Viewer.ToString());
                await userManager.AddToRoleAsync(await userManager.FindByEmailAsync(ViewerUserName), GlobalRoles.User.ToString());

                var basicUserInDb = await userManager.FindByEmailAsync(LocationUserName);
                await userManager.AddToRoleAsync(basicUserInDb, GlobalRoles.User.ToString());
                await userManager.AddClaimAsync(basicUserInDb, new System.Security.Claims.Claim(EntityRoles.Location.ToString(), location.Identifier.ToString()));

                var deviceUserInDb = await userManager.FindByEmailAsync(DeviceUserName);
                await userManager.AddToRoleAsync(deviceUserInDb, GlobalRoles.User.ToString());
                await userManager.AddClaimAsync(deviceUserInDb, new System.Security.Claims.Claim(EntityRoles.Device.ToString(), deviceWithBaseLocation.Identifier.ToString()));

                var sensors = await measurementDbContext.Sensors.ToListAsync();

                return new PrepareDbModel()
                {
                    DeviceInLocation = await measurementDbContext.Devices.Include(x => x.Sensors).FirstAsync(x => x.Id == deviceInLocation.Id),
                    DeviceUnssigned = await measurementDbContext.Devices.Include(x => x.Sensors).FirstAsync(x => x.Id == deviceWithBaseLocation.Id),
                    DeviceInLocationWithNoAccess = await measurementDbContext.Devices.Include(x => x.Sensors).FirstAsync(x => x.Id == deviceInLocationWithNoAccess.Id),
                    Location = await measurementDbContext.Locations.Include(x => x.LocationSensors).FirstAsync(x => x.Id == location.Id),
                    LocationWithNoDefinedAccess = await measurementDbContext.Locations.Include(x => x.LocationSensors).FirstAsync(x => x.Id == locationWithNoAccess.Id),
                    Sensors = sensors
                };
            }
        }

        protected class PrepareDbModel
        {
            public Device DeviceInLocation { get; set; }
            public Device DeviceUnssigned { get; set; }
            public Device DeviceInLocationWithNoAccess { get; set; }
            public Location Location { get; set; }
            public Location LocationWithNoDefinedAccess { get; set; }
            public List<Sensor> Sensors { get; set; }
        }
    }
}
