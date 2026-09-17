using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities.Enums;
using Users.Domain.Abstractions;
using Users.Infrastracture.Persistence;
using Users.Infrastracture.Seeding;

namespace Users.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class UserSeederTests(PostgreSqlFixture fixture) : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await using var context = new UsersDbContext(fixture.DbContextOptions);
        await context.Users.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task MigrateAsync_SeedsConfiguredUsers()
    {
        await using var context = CreateContext(CreateOptions());

        await context.Database.MigrateAsync();

        var users = await context.Users.OrderBy(user => user.Role).ToListAsync();
        Assert.Equal(3, users.Count);
        Assert.Equal([Role.Admin, Role.PremiumUser, Role.User], users.Select(user => user.Role));
        Assert.All(users, user => Assert.StartsWith("hashed:", user.Password.HashedValue));
    }

    [Fact]
    public void Migrate_UsesSynchronousSeeder()
    {
        using var context = CreateContext(CreateOptions());

        context.Database.Migrate();

        Assert.Equal(3, context.Users.Count());
    }

    [Fact]
    public async Task MigrateAsync_IsIdempotent()
    {
        var options = CreateOptions();

        await using (var firstContext = CreateContext(options))
            await firstContext.Database.MigrateAsync();

        await using (var secondContext = CreateContext(options))
            await secondContext.Database.MigrateAsync();

        await using var verificationContext = new UsersDbContext(fixture.DbContextOptions);
        Assert.Equal(3, await verificationContext.Users.CountAsync());
    }

    [Fact]
    public async Task MigrateAsync_DoesNothingWhenDisabled()
    {
        await using var context = CreateContext(CreateOptions(enabled: false));

        await context.Database.MigrateAsync();

        Assert.Empty(await context.Users.ToListAsync());
    }

    [Fact]
    public async Task MigrateAsync_RejectsMissingRequiredRole()
    {
        var definitions = CreateDefinitions();
        definitions[2] = definitions[2] with { Role = Role.User };
        await using var context = CreateContext(CreateOptions(definitions: definitions));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Database.MigrateAsync());

        Assert.Contains("exactly one Admin, one User, and one PremiumUser", exception.Message);
    }

    [Fact]
    public async Task MigrateAsync_RejectsDuplicateConfiguredEmails()
    {
        var definitions = CreateDefinitions();
        definitions[2] = definitions[2] with { Email = definitions[0].Email.ToUpperInvariant() };
        await using var context = CreateContext(CreateOptions(definitions: definitions));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Database.MigrateAsync());

        Assert.Contains("email addresses must be unique", exception.Message);
    }

    [Fact]
    public async Task MigrateAsync_RejectsInvalidUserData()
    {
        var definitions = CreateDefinitions();
        definitions[0] = definitions[0] with { Email = "not-an-email" };
        await using var context = CreateContext(CreateOptions(definitions: definitions));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Database.MigrateAsync());

        Assert.Contains("Invalid seed user", exception.Message);
    }

    private DbContextOptions<UsersDbContext> CreateOptions(
        bool enabled = true,
        List<UserSeedDefinition>? definitions = null)
    {
        var builder = new DbContextOptionsBuilder<UsersDbContext>();
        builder.UseNpgsql(fixture.ConnectionString, npgsql =>
            npgsql.MigrationsHistoryTable("__UsersMigrations", "Users"));
        UserSeeder.Configure(
            builder,
            new UserSeedOptions
            {
                Enabled = enabled,
                Users = definitions ?? CreateDefinitions()
            },
            FakePasswordHasher.Instance);

        return builder.Options;
    }

    private static UsersDbContext CreateContext(DbContextOptions<UsersDbContext> options) => new(options);

    private static List<UserSeedDefinition> CreateDefinitions() =>
    [
        new() { Name = "Administrator", Email = "admin@example.test", Password = "Password123", Role = Role.Admin },
        new() { Name = "Regular User", Email = "user@example.test", Password = "Password123", Role = Role.User },
        new() { Name = "Premium User", Email = "premium@example.test", Password = "Password123", Role = Role.PremiumUser }
    ];

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public static readonly FakePasswordHasher Instance = new();

        public string Hash(string plainTextPassword) => $"hashed:{plainTextPassword}";

        public bool Verify(string plainTextPassword, string hashedPassword) =>
            hashedPassword == Hash(plainTextPassword);

        public void PerformFakeVerification()
        {
        }
    }
}
