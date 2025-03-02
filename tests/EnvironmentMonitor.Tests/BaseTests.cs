using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Extensions;
using EnvironmentMonitor.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Devices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Assert = NUnit.Framework.Assert;

namespace EnvironmentMonitor.Tests
{
    [TestFixture]
    public class BaseTests
    {
        private CustomWebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private const string AdminPassword = "adminPassword#1#55";
        private const string AdminUserName = "Admin@admin.com";
        private const string ViewerPassword = "viewerPw#09#22";
        private const string ViewerUserName = "Viewer@Viewer.com";

        private CookieContainer _cookieContainer;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var integrationConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.testing.json")
                .AddEnvironmentVariables()
                .Build();
            var services = new ServiceCollection();
            services.AddInfrastructureServices(integrationConfig);
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

            await PrepareDatabase();
            _cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = _cookieContainer, AllowAutoRedirect = true };
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task AdminCanAccessDeviceInfo()
        {
            var hasLoggedIn = await LoginAsync(AdminUserName, AdminPassword);
            var path = "/api/devices/info";
            var resp = await _client.GetAsync(path);

            Assert.That(hasLoggedIn, Is.True);
            Assert.That(resp.IsSuccessStatusCode, Is.True);
        }

        [Test]
        public async Task ViewerCannotAccessDeviceInfo()
        {
            var hasLoggedIn = await LoginAsync(ViewerUserName, ViewerPassword);
            var path = "/api/devices/info";
            var resp = await _client.GetAsync(path);

            Assert.That(hasLoggedIn, Is.True);
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        private async Task<bool> LoginAsync(string email, string password)
        {
            var loginData = new
            {
                Email = email,
                Password = password,
                RememberMe = true
            };

            var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");
            // Act: Perform login (cookies will be auto-managed)
            var loginResponse = await _client.PostAsync("/api/Authentication/login", content);
            return loginResponse.IsSuccessStatusCode;
        }

        private async Task PrepareDatabase()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var adminUser = await userManager.CreateAsync(new ApplicationUser() { UserName = AdminUserName, Email = AdminUserName }, AdminPassword);
                var viewerUser = await userManager.CreateAsync(new ApplicationUser() { UserName = ViewerUserName, Email = ViewerUserName }, ViewerPassword);

                await userManager.AddToRoleAsync(await userManager.FindByEmailAsync(AdminUserName), GlobalRoles.Admin.ToString());
                await userManager.AddToRoleAsync(await userManager.FindByEmailAsync(AdminUserName), GlobalRoles.User.ToString());

                await userManager.AddToRoleAsync(await userManager.FindByEmailAsync(ViewerUserName), GlobalRoles.Viewer.ToString());
                await userManager.AddToRoleAsync(await userManager.FindByEmailAsync(ViewerUserName), GlobalRoles.User.ToString());
            }
        }
    }
}
