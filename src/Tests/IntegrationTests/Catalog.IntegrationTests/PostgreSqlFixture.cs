using Catalog.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Catalog.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("catalog_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    internal ServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await container.StartAsync();
        Services = new ServiceCollection()
            .AddLogging()
            .AddCatalogModule(container.GetConnectionString())
            .BuildServiceProvider();
        await Services.MigrateCatalogAsync();
    }

    public async Task DisposeAsync()
    {
        await Services.DisposeAsync();
        await container.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "Catalog PostgreSQL";
}
