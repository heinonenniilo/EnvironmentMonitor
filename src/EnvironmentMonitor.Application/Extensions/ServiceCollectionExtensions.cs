using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EnvironmentMonitor.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IMeasurementService, MeasurementService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IDeviceEmailService, DeviceEmailService>();
            services.AddScoped<IDeviceCommandService, DeviceCommandService>();
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}