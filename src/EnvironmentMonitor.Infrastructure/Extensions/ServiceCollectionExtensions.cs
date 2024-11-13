using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, string? connectionString = null)
        {
            // Configure DbContext
            services.AddDbContext<MeasurementDbContext>(options =>
            {
                var connectionStringToUse = connectionString ?? configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionStringToUse,
                    builder => builder.MigrationsAssembly(typeof(MeasurementDbContext).Assembly.FullName));
            });
            // Register the single repository
            services.AddScoped<IMeasurementRepository, MeasurementRepository>();

            // Add other services as needed

            return services;
        }
    }
}
