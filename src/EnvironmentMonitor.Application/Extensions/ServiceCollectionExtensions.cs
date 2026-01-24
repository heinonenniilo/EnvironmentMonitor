using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Application.Services;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EnvironmentMonitor.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration, DeviceSettings? deviceSettings = null)
        {
            services.AddScoped<IMeasurementService, MeasurementService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IDeviceEmailService, DeviceEmailService>();
            services.AddScoped<IDeviceCommandService, DeviceCommandService>();
            services.AddScoped<IUserCookieService, UserCookieService>();
            services.AddScoped<IApiKeyService, ApiKeyService>();
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            if (deviceSettings != null)
            {
                services.AddSingleton(deviceSettings);
            }
            else
            {
                var bound = new DeviceSettings();
                configuration.GetSection("DeviceSettings").Bind(bound);
                services.AddSingleton(bound);
            }

            return services;
        }
    }
}