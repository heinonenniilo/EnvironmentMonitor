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
        services.AddInfrastructureServices(opt.Configuration, opt.Configuration.GetValue<string>("DefaultConnection"), hubSettings);
        services.AddApplicationServices(opt.Configuration);
    })
    .Build();
host.Run();