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
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration, DeviceSettings? deviceSettings = null, ApiKeySettings? apiKeySettings = null)
        {
            services.AddScoped<IMeasurementService, MeasurementService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IDeviceSensorService, DeviceSensorService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IDeviceEmailService, DeviceEmailService>();
            services.AddScoped<IDeviceCommandService, DeviceCommandService>();
            services.AddScoped<ILocationCommandService, LocationCommandService>();
            services.AddScoped<IUserCookieService, UserCookieService>();
            services.AddScoped<IApiKeyService, ApiKeyService>();
            services.AddScoped<IFmiMeasurementService, FmiMeasurementService>();
            services.AddScoped<IPublicSensorService, PublicSensorService>();
            services.AddScoped<IMeasurementAnalyzeService, MeasurementAnalyzeService>();
            services.AddScoped<ISyncService, SyncService>();
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

            if (apiKeySettings != null)
            {
                services.AddSingleton(apiKeySettings);
            }
            else
            {
                var bound = new ApiKeySettings();
                configuration.GetSection("ApiKeySettings").Bind(bound);
                services.AddSingleton(bound);
            }

            // Bind SyncSettings
            var syncSettings = new SyncSettings();
            configuration.GetSection("SyncSettings").Bind(syncSettings);
            services.AddSingleton(syncSettings);

            return services;
        }
    }
}