using Microsoft.Extensions.Options;
using SharedKernal.Results;
using SharedKernel.Entities.Enums;
using Users.Application.Abstractions;
using Users.Domain;
using Users.Domain.Abstractions;
using Users.Infrastracture.Seeding;

namespace Users.IntegrationTests;

public sealed class UserSeederTests
{
    [Fact]
    public async Task SeedAsync_AddsTheThreeConfiguredUsers()
    {
        var repository = new FakeUserRepository();

        await CreateSeeder(repository).SeedAsync();

        Assert.Equal(3, repository.Users.Count);
        Assert.Equal(1, repository.SaveCalls);
        Assert.Equal(
            [Role.Admin, Role.User, Role.PremiumUser],
            repository.Users.Select(user => user.Role));
        Assert.All(repository.Users, user => Assert.StartsWith("hashed:", user.Password.HashedValue));
    }

    [Fact]
    public async Task SeedAsync_IsIdempotentByNormalizedEmail()
    {
        var repository = new FakeUserRepository();
        var seeder = CreateSeeder(repository);

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        Assert.Equal(3, repository.Users.Count);
        Assert.Equal(1, repository.SaveCalls);
    }

    [Fact]
    public async Task SeedAsync_DoesNothingWhenDisabled()
    {
        var repository = new FakeUserRepository();
        var options = Options.Create(new UserSeedOptions { Enabled = false });

        await new UserSeeder(repository, FakePasswordHasher.Instance, options).SeedAsync();

        Assert.Empty(repository.Users);
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task SeedAsync_RejectsMissingRequiredRole()
    {
        var repository = new FakeUserRepository();
        var definitions = CreateDefinitions();
        definitions[2] = definitions[2] with { Role = Role.User };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateSeeder(repository, definitions).SeedAsync());

        Assert.Contains("exactly one Admin, one User, and one PremiumUser", exception.Message);
        Assert.Empty(repository.Users);
    }

    [Fact]
    public async Task SeedAsync_RejectsDuplicateConfiguredEmails()
    {
        var repository = new FakeUserRepository();
        var definitions = CreateDefinitions();
        definitions[2] = definitions[2] with { Email = definitions[0].Email.ToUpperInvariant() };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateSeeder(repository, definitions).SeedAsync());

        Assert.Contains("email addresses must be unique", exception.Message);
        Assert.Empty(repository.Users);
    }

    [Fact]
    public async Task SeedAsync_RejectsInvalidUserData()
    {
        var repository = new FakeUserRepository();
        var definitions = CreateDefinitions();
        definitions[0] = definitions[0] with { Email = "not-an-email" };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateSeeder(repository, definitions).SeedAsync());

        Assert.Contains("Invalid seed user", exception.Message);
        Assert.Empty(repository.Users);
    }

    private static UserSeeder CreateSeeder(
        FakeUserRepository repository,
        List<UserSeedDefinition>? definitions = null)
    {
        var options = Options.Create(new UserSeedOptions
        {
            Enabled = true,
            Users = definitions ?? CreateDefinitions()
        });

        return new UserSeeder(repository, FakePasswordHasher.Instance, options);
    }

    private static List<UserSeedDefinition> CreateDefinitions() =>
    [
        new() { Name = "Administrator", Email = "admin@example.test", Password = "Password123", Role = Role.Admin },
        new() { Name = "Regular User", Email = "user@example.test", Password = "Password123", Role = Role.User },
        new() { Name = "Premium User", Email = "premium@example.test", Password = "Password123", Role = Role.PremiumUser }
    ];

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Users { get; } = [];
        public int SaveCalls { get; private set; }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Users.SingleOrDefault(user => user.Id == id));

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(Users.SingleOrDefault(user =>
                string.Equals(user.Email.Value, email.Trim(), StringComparison.OrdinalIgnoreCase)));

        public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<User>>(Users);

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }

        public void Update(User user)
        {
        }

        public void Remove(User user) => Users.Remove(user);

        public Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            return Task.FromResult<Result<int>>(Users.Count);
        }
    }

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
