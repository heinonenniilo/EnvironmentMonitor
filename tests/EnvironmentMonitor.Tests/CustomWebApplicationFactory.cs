using EnvironmentMonitor.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        }
    }
}
