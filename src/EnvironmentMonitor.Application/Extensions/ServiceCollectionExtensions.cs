using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure DbContext
            services.AddScoped<IMeasurementService, MeasurementService>();
            // Add other services as needed
            return services;
        }
    }
}

/*
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure DbContext
            services.AddInfrastructureServices(configuration);
            // Add other services as needed
            return services;
        }
    }
/
*/