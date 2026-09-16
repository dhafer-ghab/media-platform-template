using Microsoft.EntityFrameworkCore;
using Posts.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Posts.IntegrationTests;

public sealed class PostsDatabaseFixture : IAsyncLifetime
{
    private readonly string? _externalConnectionString =
        Environment.GetEnvironmentVariable("POSTS_TEST_CONNECTION_STRING");
    private readonly PostgreSqlContainer? _database;

    public PostsDatabaseFixture()
    {
        if (_externalConnectionString is null)
        {
            _database = new PostgreSqlBuilder("postgres:17-alpine")
                .WithDatabase("posts_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();
        }
    }

    internal PostsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PostsDbContext>()
            .UseNpgsql(_externalConnectionString ?? _database!.GetConnectionString())
            .Options;
        return new PostsDbContext(options);
    }

    public async Task InitializeAsync()
    {
        if (_database is not null)
            await _database.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_database is not null)
            await _database.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class PostsDatabaseCollection : ICollectionFixture<PostsDatabaseFixture>
{
    public const string Name = "Posts database";
}
