using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Identity;
using EnvironmentMonitor.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
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
            services.AddDbContext<MeasurementDbContext>(options =>
            {
                var connectionStringToUse = connectionString ?? configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionStringToUse,
                    builder => builder.MigrationsAssembly(typeof(MeasurementDbContext).Assembly.FullName));
            });
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionStringToUse = connectionString ?? configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionStringToUse,
                    builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            });
            services.AddScoped<IMeasurementRepository, MeasurementRepository>();
            // Identity stuff
            services.AddIdentity<ApplicationUser, ApplicationUserRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddRoles<ApplicationUserRole>()
                .AddRoleManager<RoleManager<ApplicationUserRole>>();
            services.AddScoped<IRoleManager, RoleManager>();
            services.AddScoped<IUserAuthService, UserAuthService>();

            var defaultSettings = new IotHubSettings();
            configuration.GetSection("IotHubSettings").Bind(defaultSettings);
            services.AddSingleton(defaultSettings);
            services.AddScoped<IHubMessageService, HubMessageService>();
            return services;
        }
    }
}
