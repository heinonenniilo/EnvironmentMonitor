using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

if (!AppDomain.CurrentDomain.FriendlyName.Contains("ef"))
{
    var configuration = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json")
          .AddUserSecrets<Program>()
          .Build();

    var services = new ServiceCollection();
    var timeout = configuration.GetSection("Timeout").Get<int?>();
    services.AddInfrastructureServices(configuration);
    var serviceProvider = services.BuildServiceProvider();
    await ApplyMigrationsAsync(serviceProvider, timeout ?? 300);
}

static async Task ApplyMigrationsAsync(IServiceProvider serviceProvider, int timeout)
{
    using (var scope = serviceProvider.CreateScope())
    {
        try
        {
            Console.WriteLine("Start running migrations");
            var measureDbContext = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();
            var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var connectionString = measureDbContext.Database.GetConnectionString();

            Console.WriteLine($"Apply migrations? Write y and press enter to start. ConnectionString: '{connectionString}'");

            if (Console.ReadLine()?.ToLower() == "y")
            {
                measureDbContext.Database.SetCommandTimeout(timeout);
                applicationDbContext.Database.SetCommandTimeout(timeout);
                Console.WriteLine("Migrating measurements db context");
                await measureDbContext.Database.MigrateAsync();
                Console.WriteLine("Migrating ApplicationDbContex");
                await applicationDbContext.Database.MigrateAsync();
            }
            else
            {
                Console.WriteLine("Migrations cancelled");
            }
            Console.WriteLine("Database migrated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
            throw;
        }
    }
}

class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MeasurementDbContext>
{
    public MeasurementDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<Program>()
            .Build();

        // Create DbContextOptionsBuilder
        var builder = new DbContextOptionsBuilder<MeasurementDbContext>();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Could not find a connection string named 'MeasurementDb'.");
        }

        // Configure the DbContext to use SQL Server
        builder.UseSqlServer(connectionString);

        return new MeasurementDbContext(builder.Options);
    }
}

class DesignTimeDbContextFactoryApplicationDb : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<Program>()
            .Build();

        // Create DbContextOptionsBuilder
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Could not find a connection string named 'MeasurementDb'.");
        }

        // Configure the DbContext to use SQL Server
        builder.UseSqlServer(connectionString);

        return new ApplicationDbContext(builder.Options);
    }
}
