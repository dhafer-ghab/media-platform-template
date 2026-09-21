using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Host.WebApi.ArtworkViews;
using Users.Infrastracture.Persistence;

namespace Users.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private PostgreSqlContainer? container;

    internal string ConnectionString { get; private set; } = null!;
    internal DbContextOptions<UsersDbContext> DbContextOptions { get; private set; } = null!;

    internal DbContextOptions<ArtworkViewsDbContext> ArtworkViewsOptions { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        ConnectionString = Environment.GetEnvironmentVariable("USERS_TEST_CONNECTION_STRING") ?? string.Empty;

        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            container = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("users_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await container.StartAsync();
            ConnectionString = container.GetConnectionString();
        }

        DbContextOptions = new DbContextOptionsBuilder<UsersDbContext>()
            .UseNpgsql(ConnectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__UsersMigrations", "Users"))
            .Options;

        ArtworkViewsOptions = new DbContextOptionsBuilder<ArtworkViewsDbContext>()
            .UseNpgsql(container.GetConnectionString(), npgsql =>
                npgsql.MigrationsHistoryTable("__ArtworkViewsMigrations", "analytics"))
            .Options;

        await using var context = new UsersDbContext(DbContextOptions);
        await context.Database.MigrateAsync();
        await using var analyticsContext = new ArtworkViewsDbContext(ArtworkViewsOptions);
        await analyticsContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => container?.DisposeAsync().AsTask() ?? Task.CompletedTask;
}

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "PostgreSQL";
}
