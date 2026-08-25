using MassTransit;
using SharedKernal.Seeding;
using Users.Infrastracture;
using Users.Presentation;

namespace Host.WebApi;

public static class HostExtensions
{

    public static async Task ApplyMigrations(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var migrators = scope.ServiceProvider
            .GetServices<IDatabaseMigrator>()
            .OrderBy(migrator => migrator.Order)
            .ThenBy(migrator => migrator.GetType().FullName, StringComparer.Ordinal);

        foreach (var migrator in migrators)
            await migrator.MigrateAsync();
    }

    public static TBuilder RegisterModules<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // register users modules
        builder.Services.AddUsersInfrastructure(builder.Configuration);
        builder.Services.AddUsersPresentation();

        builder.Services.AddMessageBus();
        builder.Services.AddHostedService<DataSeedingHostedService>();

        return builder;
    }

    /// <summary>
    /// Registered once for the whole host. AddMassTransit replaces its
    /// configuration rather than merging it, so a second module calling it
    /// would silently discard the first module's consumers and endpoints.
    /// Modules contribute consumers here; they must not configure the bus.
    /// </summary>
    private static IServiceCollection AddMessageBus(this IServiceCollection services)
    {
        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();

            // Module consumers are registered here as modules gain them.

            bus.UsingInMemory((ctx, cfg) =>
            {
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }

}
