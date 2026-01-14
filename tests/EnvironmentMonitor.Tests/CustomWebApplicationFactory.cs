using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Tests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Tests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var integrationConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.testing.json")
                .AddEnvironmentVariables()
                .Build();

            builder.UseConfiguration(integrationConfig);
            builder.ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddConfiguration(integrationConfig);
            });

            // Override email client with mock
            builder.ConfigureServices(services =>
            {
                // Remove the existing IEmailClient registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IEmailClient));
                
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add mock email client
                services.AddSingleton<IEmailClient, MockEmailClient>();
            });
        }
    }
}
