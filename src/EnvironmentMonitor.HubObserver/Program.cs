using EnvironmentMonitor.Application.Extensions;
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

        // Read queue settings from configuration
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

        var connectionString = opt.Configuration.GetValue<string>("EmailSettings:connectionString");
        var recipientEmail = opt.Configuration.GetValue<string>("EmailSettings:recipientEmails");
        var senderEmail = opt.Configuration.GetValue<string>("EmailSettings:SenderAddress");
        var subjectPrefix = opt.Configuration.GetValue<string>("EmailSettings:SubjectPrefix");
        EmailSettings? emailSettings = null;
        if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(recipientEmail) && !string.IsNullOrEmpty(senderEmail))
        {
            emailSettings = new EmailSettings
            {
                ConnectionString = connectionString,
                RecipientAddresses = recipientEmail?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? [],
                SenderAddress = senderEmail,
                EmailTitlePrefix = subjectPrefix ?? ""
            };
        }
        services.AddInfrastructureServices(opt.Configuration, opt.Configuration.GetValue<string>("DefaultConnection"), hubSettings, queueSettings, emailSettings);
        services.AddApplicationServices(opt.Configuration);
    })
    .Build();
host.Run();