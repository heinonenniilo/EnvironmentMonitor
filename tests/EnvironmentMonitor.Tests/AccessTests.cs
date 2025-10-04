using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Extensions;
using EnvironmentMonitor.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using Respawn;
using Respawn.Graph;
using System.Net;
using System.Text;
using Assert = NUnit.Framework.Assert;
using Device = EnvironmentMonitor.Domain.Entities.Device;

namespace EnvironmentMonitor.Tests
{
    [TestFixture]
    public class AccessTests
    {
        private CustomWebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private const string AdminPassword = "adminPassword#1#55";
        private const string AdminUserName = "Admin@admin.com";
        private const string ViewerPassword = "viewerPw#09#22";
        private const string ViewerUserName = "Viewer@Viewer.com";

        private const string LocationUserName = "basic@basic.fi";
        private const string LocationUserPassword = "basicPw#09#09";

        private const string DeviceUserName = "deviceuser@device.com";
        private const string DeviceUserPassword = "BasicPw#0123#2";

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

        [Test]
        public async Task AdminCanAccessDeviceInfo()
        {
            await PrepareDatabase();
            var hasLoggedIn = await LoginAsync(AdminUserName, AdminPassword);
            var path = "/api/devices/info";
            var resp = await _client.GetAsync(path);

            Assert.That(hasLoggedIn, Is.True);
            Assert.That(resp.IsSuccessStatusCode, Is.True);
        }

        [Test]
        public async Task ViewerCannotAccessDeviceInfo()
        {
            await PrepareDatabase();
            var hasLoggedIn = await LoginAsync(ViewerUserName, ViewerPassword);
            var path = "/api/devices/info";
            var resp = await _client.GetAsync(path);

            Assert.That(hasLoggedIn, Is.True);
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task LoginFailsIfIncorrectPassword()
        {
            await PrepareDatabase();
            var result = await LoginAsync(ViewerUserName, "Wrong#1Pw");
            Assert.That(result, Is.False);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task LocationGivesAccessToLocationMeasurements(bool useLocationUser)
        {
            var model = await PrepareDatabase();

            var result = useLocationUser ? await LoginAsync(LocationUserName, LocationUserPassword) : await LoginAsync(DeviceUserName, DeviceUserPassword);
            var queryDictionaryForAccessibleLocations = new List<KeyValuePair<string, string>>(); //new Dictionary<string, string>();
            var queryDictionaryForNonAccessibleLocations = new List<KeyValuePair<string, string>>();

            queryDictionaryForAccessibleLocations.Add( new KeyValuePair<string, string>("SensorIdentifiers", model.Location.Identifier.ToString()));
            queryDictionaryForNonAccessibleLocations.Add(new KeyValuePair<string, string>("SensorIdentifiers", model.LocationWithNoDefinedAccess.Identifier.ToString()));

            queryDictionaryForAccessibleLocations.Add(new KeyValuePair<string, string>("From", DateTime.Now.ToString("yyyy-MM-dd")));
            queryDictionaryForNonAccessibleLocations.Add(new KeyValuePair<string, string>("From", DateTime.Now.ToString("yyyy-MM-dd")));
            var apiPath = "/api/measurements/bylocation";
            var expectedAccessiblePathWithQueryParams = QueryHelpers.AddQueryString(apiPath, queryDictionaryForAccessibleLocations);
            var expectedNonAccessiblePathWithQueryParams = QueryHelpers.AddQueryString(apiPath, queryDictionaryForNonAccessibleLocations);
            var responseCreatedLocation = await _client.GetAsync(expectedAccessiblePathWithQueryParams);
            var responseDefaultLocation = await _client.GetAsync(expectedNonAccessiblePathWithQueryParams);

            Assert.Multiple(() =>
            {
                Assert.That(responseCreatedLocation.StatusCode, Is.EqualTo(useLocationUser ? HttpStatusCode.OK : HttpStatusCode.Forbidden));
                Assert.That(responseDefaultLocation.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            });
        }

        [Test]
        public async Task LocationGivesAccessToMeasurementsOfDevicesInLocation()
        {
            var model = await PrepareDatabase();
            var expectedAccessibleDevice = model.DeviceInLocation;
            var expectedNonAccessibleDevice = model.DeviceUnssigned;

            var result = await LoginAsync(LocationUserName, LocationUserPassword);
            var queryDictionaryForAccessibleSensors = new List<KeyValuePair<string, string>>();
            var queryDictionaryForNonAccessibleSensors = new List<KeyValuePair<string, string>>();
            foreach (var sensor in expectedAccessibleDevice.Sensors)
            {
                queryDictionaryForAccessibleSensors.Add(new KeyValuePair<string, string>("SensorIdentifiers", sensor.Identifier.ToString()));
            }
            foreach (var sensor in expectedNonAccessibleDevice.Sensors)
            {
                queryDictionaryForNonAccessibleSensors.Add(new KeyValuePair<string, string>("SensorIdentifiers", sensor.Identifier.ToString()));
            }
            foreach (var sensor in model.DeviceInLocationWithNoAccess.Sensors)
            {
                queryDictionaryForNonAccessibleSensors.Add(new KeyValuePair<string, string>("SensorIdentifiers", sensor.Identifier.ToString()));
            }

            queryDictionaryForAccessibleSensors.Add(new KeyValuePair<string, string>("From", DateTime.Now.ToString("yyyy-MM-dd")));
            queryDictionaryForNonAccessibleSensors.Add(new KeyValuePair<string, string>("From", DateTime.Now.ToString("yyyy-MM-dd")));

            var apiPath = "/api/measurements/bysensor";

            var expectedAccessiblePathWithQueryParams = QueryHelpers.AddQueryString(apiPath, queryDictionaryForAccessibleSensors);
            var expectedNonAccessiblePathWithQueryParams = QueryHelpers.AddQueryString(apiPath, queryDictionaryForNonAccessibleSensors);
            var responseOkExpected = await _client.GetAsync(expectedAccessiblePathWithQueryParams);
            var responseForbiddenExpected = await _client.GetAsync(expectedNonAccessiblePathWithQueryParams);

            Assert.Multiple(() =>
            {
                Assert.That(responseOkExpected.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(responseForbiddenExpected.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            });
        }

        [Test]
        public async Task DeviceGivesAccessToMeasurementsOfDeviceSensors()
        {
            var model = await PrepareDatabase();
            var expectedAccessibleDevice = model.DeviceUnssigned;
            var expectedNonAccessibleDevice = model.DeviceInLocation;

            var result = await LoginAsync(DeviceUserName, DeviceUserPassword);
            var queryDictionaryForAccessibleSensors = new Dictionary<string, string>();
            var queryDictionaryForNonAccessibleSensors = new Dictionary<string, string>();
            foreach (var sensor in expectedAccessibleDevice.Sensors)
            {
                queryDictionaryForAccessibleSensors.Add("SensorIdentifiers", sensor.Identifier.ToString());
            }
            foreach (var sensor in expectedNonAccessibleDevice.Sensors)
            {
                queryDictionaryForNonAccessibleSensors.Add("SensorIdentifiers", sensor.Identifier.ToString());
            }
            queryDictionaryForAccessibleSensors.Add("From", DateTime.Now.ToString("yyyy-MM-dd"));
            queryDictionaryForNonAccessibleSensors.Add("From", DateTime.Now.ToString("yyyy-MM-dd"));

            var apiPath = "/api/measurements/bysensor";

            var expectedAccessiblePathWithQueryParams = QueryHelpers.AddQueryString(apiPath, queryDictionaryForAccessibleSensors);
            var expectedNonAccessiblePathWithQueryParams = QueryHelpers.AddQueryString(apiPath, queryDictionaryForNonAccessibleSensors);
            var responseOkExpected = await _client.GetAsync(expectedAccessiblePathWithQueryParams);
            var responseForbiddenExpected = await _client.GetAsync(expectedNonAccessiblePathWithQueryParams);

            Assert.Multiple(() =>
            {
                Assert.That(responseOkExpected.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(responseForbiddenExpected.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            });
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task AdminAndViewerCanSeeMeasurementsFromAllLocations(bool isAdmin)
        {
            var model = await PrepareDatabase();
            var loginResult = isAdmin ? await LoginAsync(AdminUserName, AdminPassword) : await LoginAsync(ViewerUserName, ViewerPassword);
            var queryParams =  new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("SensorIdentifiers", model.Location.Identifier.ToString()),
                new KeyValuePair<string, string>( "SensorIdentifiers", model.LocationWithNoDefinedAccess.Identifier.ToString() ),
                new KeyValuePair<string, string>("From", DateTime.Now.ToString("yyyy-MM-dd"))
            };
            var apiPath = "/api/measurements/bylocation";
            var clientPath = QueryHelpers.AddQueryString(apiPath, queryParams);
            var responseOkExpected = await _client.GetAsync(clientPath);

            Assert.That(responseOkExpected.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.OK));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task AdminAndViewerCanSeeMeasurementsFromAllSensors(bool isAdmin)
        {
            var model = await PrepareDatabase();
            var loginResult = isAdmin ? await LoginAsync(AdminUserName, AdminPassword) : await LoginAsync(ViewerUserName, ViewerPassword);
            var queryParams = model.Sensors.Select(x => new KeyValuePair<string, string>("SensorIdentifiers", x.Identifier.ToString())).ToList();
            queryParams.Add(new KeyValuePair<string, string>("From", DateTime.Now.ToString("yyyy-MM-dd")));
            var apiPath = "/api/measurements/bysensor";
            var clientPath = QueryHelpers.AddQueryString(apiPath, queryParams);
            var responseOkExpected = await _client.GetAsync(clientPath);

            Assert.That(responseOkExpected.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        private async Task<bool> LoginAsync(string email, string password)
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

        private async Task<PrepareDbModel> PrepareDatabase()
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
                        Name = "Test-01",
                        SensorId = 1
                    }],
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

        private class PrepareDbModel
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
