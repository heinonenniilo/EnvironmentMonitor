using Azure.Identity;
using Azure.Storage.Queues;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Identity;
using EnvironmentMonitor.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EnvironmentMonitor.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private const string PostgreSqlMigrationsAssembly = "EnvironmentMonitor.PostgreSqlMigrations";

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
            IConfiguration configuration,
            string? connectionString = null,
            IotHubSettings? iotHubSettings = null,
            QueueSettings? queueSettings = null,
            EmailSettings? emailSettings = null,
            ApplicationSettings? applicationSettings = null,
            DataProtectionKeysSettings? dataProtectionKeysSettings = null,
            DatabaseSettings? databaseSettings = null
            )
        {
            var connectionStringToUse = connectionString ?? configuration.GetConnectionString("DefaultConnection");

            DatabaseSettings databaseSettingsToUse;
            if (databaseSettings != null)
            {
                databaseSettingsToUse = databaseSettings;
            }
            else
            {
                databaseSettingsToUse = new DatabaseSettings();
                configuration.GetSection("DatabaseSettings").Bind(databaseSettingsToUse);
            }

            var databaseProvider = databaseSettingsToUse.Provider;

            // Register DatabaseSettings as singleton
            services.AddSingleton(databaseSettingsToUse);

            // Needed by WebApi's ICurrentUser implementation; harmless in non-web hosts.
            services.AddHttpContextAccessor();

            services.AddDbContext<MeasurementDbContext>(options =>
            {
                ConfigureDatabase(options, databaseProvider, connectionStringToUse);
            });
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                ConfigureDatabase(options, databaseProvider, connectionStringToUse);
            });
            services.AddDbContext<DataProtectionKeysContext>(options =>
            {
                ConfigureDatabase(options, databaseProvider, connectionStringToUse);
            });
            
            DataProtectionKeysSettings? dataProtectionKeysSettingsToCheck;
            if (dataProtectionKeysSettings != null)
            {
                services.AddSingleton(dataProtectionKeysSettings);
                dataProtectionKeysSettingsToCheck = dataProtectionKeysSettings;
            }
            else
            {
                var defaultDataProtectionKeysSettings = new DataProtectionKeysSettings();
                configuration.GetSection("DataProtectionKeysSettings").Bind(defaultDataProtectionKeysSettings);
                services.AddSingleton(defaultDataProtectionKeysSettings);
                dataProtectionKeysSettingsToCheck = configuration.GetSection("DataProtectionKeysSettings").Get<DataProtectionKeysSettings>();
            }
 
            if (dataProtectionKeysSettingsToCheck != null && dataProtectionKeysSettingsToCheck.StoreInDatabase)
            {
                var dataProtectionBuilder = services.AddDataProtection()
                    .PersistKeysToDbContext<DataProtectionKeysContext>()
                    .SetApplicationName("EnvironmentMonitor");

                if (dataProtectionKeysSettingsToCheck.EncryptWithKeyVault && 
                    !string.IsNullOrEmpty(dataProtectionKeysSettingsToCheck.KeyVaultKeyIdentifier))
                {
                    // Use ClientSecretCredential if all required properties are provided
                    if (!string.IsNullOrEmpty(dataProtectionKeysSettingsToCheck.TenantId) &&
                        !string.IsNullOrEmpty(dataProtectionKeysSettingsToCheck.ClientId) &&
                        !string.IsNullOrEmpty(dataProtectionKeysSettingsToCheck.ClientSecret))
                    {
                        var credential = new ClientSecretCredential(
                            dataProtectionKeysSettingsToCheck.TenantId,
                            dataProtectionKeysSettingsToCheck.ClientId,
                            dataProtectionKeysSettingsToCheck.ClientSecret);

                        dataProtectionBuilder.ProtectKeysWithAzureKeyVault(
                            new Uri(dataProtectionKeysSettingsToCheck.KeyVaultKeyIdentifier),
                            credential);
                    }
                    else
                    {
                        dataProtectionBuilder.ProtectKeysWithAzureKeyVault(
                            new Uri(dataProtectionKeysSettingsToCheck.KeyVaultKeyIdentifier),
                            new DefaultAzureCredential());
                    }
                }
            }

            services.AddScoped<IMeasurementRepository, MeasurementRepository>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<ISensorRepository, SensorRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<IStatusVariableRepository, StatusVariableRepository>();
            services.AddScoped<IPublicSensorRepository, PublicSensorRepository>();
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
            services.AddSingleton<IApiKeyHashService, ApiKeyHashService>();
            services.AddSingleton<IIdentifierGenerator, IdentifierGenerator>();
            services.AddSingleton<ICacheService, CacheService>();

            services.AddDistributedMemoryCache();

            // Identity stuff
            services.AddIdentity<ApplicationUser, ApplicationUserRole>(options =>
            {
                // Require confirmed email to sign in
                options.SignIn.RequireConfirmedEmail = true;
            })
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

            if (applicationSettings != null)
            {
                services.AddSingleton(applicationSettings);
            }
            else
            {
                var defaultAppSettings = new ApplicationSettings();
                configuration.GetSection("ApplicationSettings").Bind(defaultAppSettings);
                services.AddSingleton(defaultAppSettings);
            }

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

            EmailSettings emailSettingsToUse;
            if (emailSettings != null)
            {
                services.AddSingleton(emailSettings);
                emailSettingsToUse = emailSettings;
            }
            else
            {
                var defaultEmailSettings = new EmailSettings();
                configuration.GetSection("EmailSettings").Bind(defaultEmailSettings);
                services.AddSingleton(defaultEmailSettings);
                emailSettingsToUse = defaultEmailSettings;
            }

            services.AddSingleton<IDateService, DateService>();
            services.AddSingleton<IHubMessageService, HubMessageService>();
            services.AddSingleton<IStorageClient, StorageClient>();
            services.AddSingleton<IImageService, ImageService>();
            services.AddScoped<IPaginationService, PaginationService>();
            services.AddSingleton<IKeyVaultClient, KeyVaultClient>();
            services.AddSingleton<IQueueClient, Services.QueueClient>();

            services.AddHttpClient("MailGun");
            
            // Register FMI Weather Client
            services.AddHttpClient<IFmiWeatherClient, FmiWeatherClient>();

            switch (emailSettingsToUse.ClientType)
            {
                case EmailClientTypes.Smtp:
                    services.AddSingleton<IEmailClient, SmtpEmailClient>();
                    break;
                case EmailClientTypes.MailGun:
                    services.AddSingleton<IEmailClient, MailGunEmailClient>();
                    break;
                case EmailClientTypes.AzureCommunicationService:
                default:
                    services.AddSingleton<IEmailClient, AzureCommunicationServiceClient>();
                    break;
            }

            return services;
        }

        public static void ConfigureDatabase(
            DbContextOptionsBuilder options,
            string databaseProvider,
            string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            }

            if (databaseProvider.Equals(DatabaseSettings.PostgreSql, StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString,
                    builder => builder.MigrationsAssembly(PostgreSqlMigrationsAssembly));
                return;
            }

            if (databaseProvider.Equals(DatabaseSettings.SqlServer, StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString,
                    builder => builder.MigrationsAssembly(typeof(MeasurementDbContext).Assembly.FullName));
                return;
            }

            throw new InvalidOperationException(
                $"Unsupported database provider '{databaseProvider}'. Use '{DatabaseSettings.SqlServer}' or '{DatabaseSettings.PostgreSql}'.");
        }

        public static IServiceCollection AddHangfireServices(
            this IServiceCollection services,
            IConfiguration configuration,
            string? hangfireConnectionString = null,
            DatabaseSettings? databaseSettings = null,
            bool addServer = true)
        {
            var hangfireConnectionStringToUse = hangfireConnectionString ?? configuration.GetConnectionString("HangfireConnection");

            if (string.IsNullOrWhiteSpace(hangfireConnectionStringToUse))
            {
                throw new InvalidOperationException("Hangfire connection string 'HangfireConnection' is not configured.");
            }

            DatabaseSettings databaseSettingsToUse;
            if (databaseSettings != null)
            {
                databaseSettingsToUse = databaseSettings;
            }
            else
            {
                databaseSettingsToUse = new DatabaseSettings();
                configuration.GetSection("DatabaseSettings").Bind(databaseSettingsToUse);
            }

            var databaseProvider = databaseSettingsToUse.Provider;

            // Configure Hangfire storage based on database provider
            if (databaseProvider.Equals(DatabaseSettings.PostgreSql, StringComparison.OrdinalIgnoreCase))
            {
                services.AddHangfire(config => config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(options =>
                        options.UseNpgsqlConnection(hangfireConnectionStringToUse)));
            }
            else if (databaseProvider.Equals(DatabaseSettings.SqlServer, StringComparison.OrdinalIgnoreCase))
            {
                services.AddHangfire(config => config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(hangfireConnectionStringToUse));
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unsupported database provider '{databaseProvider}' for Hangfire. Use '{DatabaseSettings.SqlServer}' or '{DatabaseSettings.PostgreSql}'.");
            }

            if (addServer)
            {
                services.AddHangfireServer();
            }

            return services;
        }
    }
}
