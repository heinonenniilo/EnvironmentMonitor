using EnvironmentMonitor.Application.Extensions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Infrastructure.Extensions;
using EnvironmentMonitor.Worker;
using EnvironmentMonitor.Worker.Jobs;
using EnvironmentMonitor.Worker.Services;
using Hangfire;

var builder = Host.CreateApplicationBuilder(args);

// Register CurrentUser service for background job context
builder.Services.AddSingleton<ICurrentUser, CurrentUser>();

// Add Infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Application services
builder.Services.AddApplicationServices(builder.Configuration);

// Add Hangfire services (includes server for job processing)
builder.Services.AddHangfireServices(builder.Configuration, addServer: true);

// Register Hangfire job classes
builder.Services.AddScoped<FmiMeasurementSyncJob>();

// Add KeepAlive background service
builder.Services.AddHostedService<Worker>();

// Add systemd support for running as Linux service
builder.Services.AddSystemd();

var host = builder.Build();

// Register all recurring Hangfire jobs using DI
using (var scope = host.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    HangfireJobRegistration.RegisterJobs(builder.Configuration, recurringJobManager);
}

host.Run();
