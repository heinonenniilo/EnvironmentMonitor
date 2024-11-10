using EnvironmentMonitor.Application.Extensions;
using EnvironmentMonitor.Infrastructure.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(EnvironmentMonitor.HubListener.Startup))]

namespace EnvironmentMonitor.HubListener;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging();
        var configuration = builder.GetContext().Configuration;
        builder.Services.AddInfrastructureServices(configuration, configuration.GetValue<string>("DefaultConnection")) ;
        builder.Services.AddApplicationServices(builder.GetContext().Configuration);
    }
}