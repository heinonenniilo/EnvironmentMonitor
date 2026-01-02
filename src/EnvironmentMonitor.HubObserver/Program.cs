using EnvironmentMonitor.Application.Extensions;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.HubObserver.Services;
using EnvironmentMonitor.Infrastructure.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((opt, services) =>
    {
        services.AddSingleton<ICurrentUser, CurrentUser>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var hubDomain = opt.Configuration.GetValue<string>("HubDomain");
        IotHubSettings? hubSettings = null;

        if (!string.IsNullOrEmpty(hubDomain))
        {
            hubSettings = new IotHubSettings
            {
                IotHubDomain = hubDomain,
                ConnectionString = ""
            };
        }

        var queueServiceUri = opt.Configuration.GetValue<string>("StorageAccountConnection:queueServiceUri");
        var accountName = opt.Configuration.GetValue<string>("StorageAccountConnection:accountName");
        var defaultQueueName = opt.Configuration.GetValue<string>("DeviceMessagesQueueName");

        QueueSettings? queueSettings = null;
        if (!string.IsNullOrEmpty(queueServiceUri) || !string.IsNullOrEmpty(accountName))
        {
            queueSettings = new QueueSettings
            {
                QueueServiceUri = queueServiceUri,
                AccountName = accountName,
                DefaultQueueName = defaultQueueName
            };
        }

        var emailSettings = new EmailSettings();
        opt.Configuration.GetSection("EmailSettings").Bind(emailSettings);

        var baseUrl = opt.Configuration.GetValue<string>("ApplicationSettings:BaseUrl");
        var applicationSettings = new ApplicationSettings { BaseUrl = baseUrl ?? "" };

        var storeInDatabase = opt.Configuration.GetValue<bool>("DataProtectionKeysSettings:StoreInDatabase");
        var encryptWithKeyVault = opt.Configuration.GetValue<bool>("DataProtectionKeysSettings:EncryptWithKeyVault");
        var keyVaultKeyIdentifier = opt.Configuration.GetValue<string>("DataProtectionKeysSettings:KeyVaultKeyIdentifier");

        var dataProtectionKeysSettings = new DataProtectionKeysSettings
        {
            StoreInDatabase = storeInDatabase,
            EncryptWithKeyVault = encryptWithKeyVault,
            KeyVaultKeyIdentifier = keyVaultKeyIdentifier ?? ""
        };

        services.AddInfrastructureServices(opt.Configuration, opt.Configuration.GetValue<string>("DefaultConnection"), hubSettings, queueSettings, emailSettings, applicationSettings, dataProtectionKeysSettings);
        services.AddApplicationServices(opt.Configuration);
    })
    .Build();
host.Run();