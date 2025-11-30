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
    public class AccessTests : BaseIntegrationTest
    {
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
            var queryDictionaryForAccessibleLocations = new List<KeyValuePair<string, string>>();
            var queryDictionaryForNonAccessibleLocations = new List<KeyValuePair<string, string>>();

            queryDictionaryForAccessibleLocations.Add( new KeyValuePair<string, string>("LocationIdentifiers", model.Location.Identifier.ToString()));
            queryDictionaryForNonAccessibleLocations.Add(new KeyValuePair<string, string>("LocationIdentifiers", model.LocationWithNoDefinedAccess.Identifier.ToString()));

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
        [TestCase(true)]
        [TestCase(false)]
        public async Task AdminAndViewerCanSeeMeasurementsFromAllLocations(bool isAdmin)
        {
            var model = await PrepareDatabase();
            var loginResult = isAdmin ? await LoginAsync(AdminUserName, AdminPassword) : await LoginAsync(ViewerUserName, ViewerPassword);
            var queryParams =  new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("LocationIdentifiers", model.Location.Identifier.ToString()),
                new KeyValuePair<string, string>( "LocationIdentifiers", model.LocationWithNoDefinedAccess.Identifier.ToString() ),
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
    }
}
