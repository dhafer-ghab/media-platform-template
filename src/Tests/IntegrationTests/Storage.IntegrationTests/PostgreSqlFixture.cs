using Microsoft.EntityFrameworkCore;
using Storage.Infrastracture.Persistence;
using Testcontainers.PostgreSql;

namespace Storage.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("storage_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    internal DbContextOptions<StorageDbContext> DbContextOptions { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        DbContextOptions = new DbContextOptionsBuilder<StorageDbContext>()
            .UseNpgsql(_container.GetConnectionString(), npgsql =>
                npgsql.MigrationsHistoryTable("__StorageMigrations", "Storage"))
            .Options;

        await using var context = new StorageDbContext(DbContextOptions);
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "PostgreSQL";
}
