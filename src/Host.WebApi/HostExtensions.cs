using MassTransit;
using Microsoft.EntityFrameworkCore;
using Host.WebApi.ArtworkViews;
using Users.Infrastracture.Persistence;
using Users.Infrastracture;
using Users.Presentation;

namespace Host.WebApi;

public static class HostExtensions
{

    public static async Task ApplyMigrations(this WebApplication app)
    {
        await MigrateModuleDbAsync<UsersDbContext>(app);
        await MigrateModuleDbAsync<ArtworkViewsDbContext>(app);
    }

    public static TBuilder RegisterModules<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // register users modules
        builder.Services.AddUsersInfrastructure(builder.Configuration);
        builder.Services.AddUsersPresentation();

        builder.Services.AddMessageBus();

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
}
