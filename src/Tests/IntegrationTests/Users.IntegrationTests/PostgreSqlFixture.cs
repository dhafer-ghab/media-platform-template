using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Users.Infrastracture.Persistence;

namespace Users.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private PostgreSqlContainer? container;

    internal string ConnectionString { get; private set; } = null!;
    internal DbContextOptions<UsersDbContext> DbContextOptions { get; private set; } = null!;

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

        await using var context = new UsersDbContext(DbContextOptions);
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => container?.DisposeAsync().AsTask() ?? Task.CompletedTask;
}

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "PostgreSQL";
}
