using MassTransit;
using Microsoft.EntityFrameworkCore;
using Storage.Infrastracture.Persistence;
using Users.Infrastracture.Persistence;
using Users.Infrastracture;
using Storage.Infrastracture;
using Storage.Presentation;
using Users.Presentation;

namespace Host.WebApi;

public static class HostExtensions
{

    public static async Task ApplyMigrations(this WebApplication app)
    {
        await MigrateModuleDbAsync<UsersDbContext>(app);
        await MigrateModuleDbAsync<StorageDbContext>(app);
    }

    public static TBuilder RegisterModules<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // register users module
        builder.Services.AddUsersInfrastructure(builder.Configuration);
        builder.Services.AddUsersPresentation();

        builder.Services.AddMessageBus();

        // storage module
        builder.Services.AddStorageInfrastructure(builder.Configuration);
        builder.Services.AddStoragePresentation();


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


    private static async Task MigrateModuleDbAsync<TDbContext>(this WebApplication app) where TDbContext : DbContext
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await db.Database.MigrateAsync();
    }

    public static async Task MigrateStorageDbAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<StorageDbContext>();

        await db.Database.MigrateAsync();
    }
}
