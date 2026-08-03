using EnvironmentMonitor.Application.Extensions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Infrastructure.Extensions;
using EnvironmentMonitor.Worker;
using EnvironmentMonitor.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Register CurrentUser service for background job context
builder.Services.AddSingleton<ICurrentUser, CurrentUser>();

// Configure connection strings and database settings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var hangfireConnectionString = builder.Configuration.GetConnectionString("HangfireConnection");

// Add Infrastructure services
builder.Services.AddInfrastructureServices(
    builder.Configuration,
    connectionString: connectionString);

// Add Application services
builder.Services.AddApplicationServices(builder.Configuration);

// Add Hangfire services (includes server for job processing)
builder.Services.AddHangfireServices(
    builder.Configuration,
    hangfireConnectionString: hangfireConnectionString,
    addServer: true);

// Add Worker service
builder.Services.AddHostedService<Worker>();

// Add systemd support for running as Linux service
builder.Services.AddSystemd();

var host = builder.Build();
host.Run();
