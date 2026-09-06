using Microsoft.EntityFrameworkCore;
using SharedKernal.Messaging;
using Users.Common;
using Users.Domain;
using Users.Domain.Abstractions;
using Users.Infrastracture.Persistence;

namespace Users.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class UserRepositoryEmailUniquenessTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task SaveChanges_WhenEmailAlreadyExists_ReturnsConflict()
    {
        var email = $"{Guid.NewGuid():N}@example.test";
        await AddUserAsync(email);
        var countBeforeConflict = await CountUsersAsync();

        await using var context = new UsersDbContext(fixture.DbContextOptions);
        var repository = new UserRepository(context, NoOpDomainEventDispatcher.Instance);
        await repository.AddAsync(CreateUser($" {email.ToUpperInvariant()} "));

        var result = await repository.SaveChangesAsync();

        Assert.True(result.IsFailure);
        Assert.Equal("User.EmailExists", result.Error.Code);
        Assert.Equal(countBeforeConflict, await CountUsersAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenUpdatedEmailAlreadyExists_ReturnsConflictWithoutPersistingUpdate()
    {
        var firstEmail = $"first-{Guid.NewGuid():N}@example.test";
        var secondEmail = $"second-{Guid.NewGuid():N}@example.test";
        var firstUserId = await AddUserAsync(firstEmail);
        await AddUserAsync(secondEmail);

        await using (var context = new UsersDbContext(fixture.DbContextOptions))
        {
            var repository = new UserRepository(context, NoOpDomainEventDispatcher.Instance);
            var user = await repository.GetByIdAsync(firstUserId);

            Assert.NotNull(user);
            Assert.True(user.UpdateProfile("Changed", secondEmail).IsSuccess);

            repository.Update(user);
            var result = await repository.SaveChangesAsync();

            Assert.True(result.IsFailure);
            Assert.Equal("User.EmailExists", result.Error.Code);
        }

        await using var verificationContext = new UsersDbContext(fixture.DbContextOptions);
        var persistedUser = await verificationContext.Users.FindAsync(firstUserId);
        Assert.NotNull(persistedUser);
        Assert.Equal(firstEmail, persistedUser.Email.Value);
        Assert.Equal("User", persistedUser.Name);
    }

    [Fact]
    public async Task Migration_CreatesUniqueEmailIndex()
    {
        await using var context = new UsersDbContext(fixture.DbContextOptions);
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM pg_indexes
            WHERE schemaname = 'Users'
              AND tablename = 'Users'
              AND indexname = 'UX_Users_Email'
              AND indexdef LIKE 'CREATE UNIQUE INDEX%';
            """;

        await context.Database.OpenConnectionAsync();
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task SaveChanges_WhenAnotherDatabaseConstraintFails_DoesNotReturnEmailConflict()
    {
        await using var context = new UsersDbContext(fixture.DbContextOptions);
        var repository = new UserRepository(context, NoOpDomainEventDispatcher.Instance);
        var user = CreateUser($"{Guid.NewGuid():N}@example.test");
        await repository.AddAsync(user);
        context.Entry(user).Property(nameof(User.Name)).CurrentValue = new string('x', 201);

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
    }

    private async Task<Guid> AddUserAsync(string email)
    {
        await using var context = new UsersDbContext(fixture.DbContextOptions);
        var repository = new UserRepository(context, NoOpDomainEventDispatcher.Instance);
        var user = CreateUser(email);
        await repository.AddAsync(user);

        var result = await repository.SaveChangesAsync();
        Assert.True(result.IsSuccess);
        return user.Id;
    }

    private async Task<int> CountUsersAsync()
    {
        await using var context = new UsersDbContext(fixture.DbContextOptions);
        return await context.Users.CountAsync();
    }

    private static User CreateUser(string email) =>
        User.Create("User", email, "Password123", Role.User, FakePasswordHasher.Instance).Value;

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public static readonly FakePasswordHasher Instance = new();

        public string Hash(string plainTextPassword) => "hash";

        public bool Verify(string plainTextPassword, string hashedPassword) => true;

        public void PerformFakeVerification()
        {
        }
    }

    private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public static readonly NoOpDomainEventDispatcher Instance = new();

        public Task DispatchAsync(
            IEnumerable<IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
