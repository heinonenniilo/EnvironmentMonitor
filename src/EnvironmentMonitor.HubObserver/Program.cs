using EnvironmentMonitor.Application.Extensions;
using EnvironmentMonitor.Infrastructure.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((opt, services) =>
    {
        // services.AddApplicationInsightsTelemetryWorkerService();
        // services.ConfigureFunctionsApplicationInsights();
        services.AddInfrastructureServices(opt.Configuration, opt.Configuration.GetValue<string>("DefaultConnection"));
        services.AddApplicationServices(opt.Configuration);
    })
    .Build();
host.Run();