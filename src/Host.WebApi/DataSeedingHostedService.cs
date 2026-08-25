using SharedKernal.Seeding;

namespace Host.WebApi;

public sealed class DataSeedingHostedService(
    IServiceProvider services,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<DataSeedingHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!configuration.GetValue<bool>("SeedData:Enabled"))
            return;

        var allowedEnvironments = configuration
            .GetSection("SeedData:AllowedEnvironments")
            .Get<string[]>() ?? [Environments.Development];
        if (!allowedEnvironments.Contains(
                environment.EnvironmentName,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Data seeding is not allowed in the '{environment.EnvironmentName}' environment.");
        }

        await using var scope = services.CreateAsyncScope();
        var seeders = scope.ServiceProvider
            .GetServices<IDataSeeder>()
            .OrderBy(seeder => seeder.Order)
            .ThenBy(seeder => seeder.GetType().FullName, StringComparer.Ordinal)
            .ToList();

        foreach (var seeder in seeders)
        {
            var seederName = seeder.GetType().Name;
            logger.LogInformation("Running data seeder {SeederName}", seederName);
            await seeder.SeedAsync(cancellationToken);
            logger.LogInformation("Completed data seeder {SeederName}", seederName);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
