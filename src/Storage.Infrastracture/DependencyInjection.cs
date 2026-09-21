using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storage.Application;
using Storage.Application.Abstractions;
using Storage.Infrastracture.BackgroundServices;
using Storage.Infrastracture.Persistence;
using Storage.Infrastracture.Storage;

namespace Storage.Infrastracture;

internal static class DependencyInjection
{
    public static IServiceCollection AddStorageInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext(configuration);

        services.AddStorageApplication(configuration);

        return services;
    }

    private static IServiceCollection AddDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreConnectionString");

        services.AddDbContextPool<StorageDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__StorageMigrations", "Storage")));

        return services;
    }

    private static IServiceCollection AddStorageApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.InitializeApplication();

        services.Configure<S3Options>(configuration.GetSection(S3Options.SectionName));
        services.AddTransient<IFileStorageService, S3FileStorageService>();
        services.AddScoped<IFileRepository, FileRepository>();

        services.AddSingleton<IFileImportQueue, FileImportQueue>();
        services.AddHostedService<FileImportBackgroundService>();

        return services;
    }
}
