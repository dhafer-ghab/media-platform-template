using Catalog.Application;
using Catalog.Application.Abstractions;
using Catalog.Infrastructure.Import;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure;

public static class CatalogModule
{
    public const string ConnectionStringName = "PostgreConnectionString";

    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is required.");

        return services.AddCatalogModule(connectionString);
    }

    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddCatalogApplication();
        services.AddDbContextPool<CatalogDbContext>(options => Configure(options, connectionString));
        services.AddScoped<ICatalogReader, CatalogReader>();
        services.AddScoped(serviceProvider =>
            new CatalogJsonImporter(serviceProvider.GetRequiredService<CatalogDbContext>()));
        return services;
    }

    public static async Task MigrateCatalogAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }

    internal static void Configure(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable("__CatalogMigrations", CatalogDbContext.Schema));
}
