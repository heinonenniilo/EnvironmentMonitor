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

            // Override services with mocks
            builder.ConfigureServices(services =>
            {
                // Remove the existing IEmailClient registration
                var emailDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IEmailClient));
                
                if (emailDescriptor != null)
                {
                    services.Remove(emailDescriptor);
                }

                // Add mock email client
                services.AddSingleton<IEmailClient, MockEmailClient>();
                var queueDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IQueueClient));
               
                if (queueDescriptor != null)
                {
                    services.Remove(queueDescriptor);
                }
                services.AddSingleton<IQueueClient, MockQueueClient>();
            });
        }
    }
}
