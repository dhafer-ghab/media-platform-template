using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Users.Infrastracture.Persistence;

namespace Users.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("users_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    internal DbContextOptions<UsersDbContext> DbContextOptions { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await container.StartAsync();

        DbContextOptions = new DbContextOptionsBuilder<UsersDbContext>()
            .UseNpgsql(container.GetConnectionString(), npgsql =>
                npgsql.MigrationsHistoryTable("__UsersMigrations", "Users"))
            .Options;

        await using var context = new UsersDbContext(DbContextOptions);
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => container.DisposeAsync().AsTask();
}

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "PostgreSQL";
}
