using Azure.Identity;
using Azure.Storage.Queues;
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
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
            IConfiguration configuration,
            string? connectionString = null,
            IotHubSettings? iotHubSettings = null,
            QueueSettings? queueSettings = null,
            EmailSettings? emailSettings = null
            )
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
            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<IDeviceEmailRepository, DeviceEmailRepository>();
            // Identity stuff
            services.AddIdentity<ApplicationUser, ApplicationUserRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddRoles<ApplicationUserRole>()
                .AddRoleManager<RoleManager<ApplicationUserRole>>();
            services.AddScoped<IRoleManager, RoleManager>();
            services.AddScoped<IUserAuthService, UserAuthService>();


            if (iotHubSettings != null)
            {
                services.AddSingleton(iotHubSettings);
            }
            else
            {
                var defaultSettings = new IotHubSettings();
                configuration.GetSection("IotHubSettings").Bind(defaultSettings);
                services.AddSingleton(defaultSettings);
            }

            var storageAccountSettings = new StorageAccountSettings();
            configuration.GetSection("StorageSettings").Bind(storageAccountSettings);
            
            services.AddSingleton(storageAccountSettings);

            var fileUploadDefaultSettings = new FileUploadSettings();
            configuration.GetSection("FileUploadSettings").Bind(fileUploadDefaultSettings);
            services.AddSingleton(fileUploadDefaultSettings);

            var keyVaultSettings = new KeyVaultSettings();
            configuration.GetSection("KeyVaultSettings").Bind(keyVaultSettings);
            services.AddSingleton(keyVaultSettings);

            if (queueSettings != null)
            {
                services.AddSingleton(queueSettings);
            }
            else
            {
                var defaultQueueSettings = new QueueSettings();
                configuration.GetSection("QueueSettings").Bind(defaultQueueSettings);
                services.AddSingleton(defaultQueueSettings);
            }

            if (emailSettings != null)
            {
                services.AddSingleton(emailSettings);
            }
            else
            {
                var defaultEmailSettings = new EmailSettings();
                configuration.GetSection("EmailSettings").Bind(defaultEmailSettings);
                services.AddSingleton(defaultEmailSettings);
            }

            services.AddSingleton<IDateService, DateService>();
            services.AddSingleton<IHubMessageService, HubMessageService>();
            services.AddSingleton<IStorageClient, StorageClient>();
            services.AddSingleton<IImageService, ImageService>();
            services.AddScoped<IPaginationService, PaginationService>();
            services.AddSingleton<IKeyVaultClient, KeyVaultClient>();
            services.AddSingleton<IQueueClient, Services.QueueClient>();
            services.AddSingleton<IEmailClient, Services.EmailClient>();

            return services;
        }
    }
}
