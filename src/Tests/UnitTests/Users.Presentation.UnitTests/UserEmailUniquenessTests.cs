using Users.Application.Abstractions;
using Users.Application.Users.Commands.CreateUser;
using Users.Application.Users.Commands.UpdateUser;
using SharedKernal.Results;
using Users.Common;
using Users.Domain;
using Users.Domain.Abstractions;

namespace Users.Presentation.UnitTests;

public sealed class UserEmailUniquenessTests
{
    [Fact]
    public async Task Create_WhenEmailExists_ReturnsConflict()
    {
        var repository = new FakeUserRepository
        {
            ExistingUser = CreateUser("user@example.test")
        };
        var handler = new CreateUserCommandHandler(repository, new FakePasswordHasher());

        var result = await handler.Handle(
            new CreateUserCommand("User", " USER@example.test ", "Password123", Role.User),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("User.EmailExists", result.Error.Code);
        Assert.Null(repository.AddedUser);
    }

    [Fact]
    public async Task Create_WhenUniqueConstraintWinsRace_ReturnsConflict()
    {
        var repository = new FakeUserRepository { ReturnConflictOnSave = true };
        var handler = new CreateUserCommandHandler(repository, new FakePasswordHasher());

        var result = await handler.Handle(
            new CreateUserCommand("User", "user@example.test", "Password123", Role.User),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("User.EmailExists", result.Error.Code);
    }

    [Fact]
    public async Task Update_WhenEmailExists_ReturnsConflict()
    {
        var user = CreateUser("current@example.test");
        var repository = new FakeUserRepository
        {
            ExistingUser = user,
            ExistingEmailUser = CreateUser("taken@example.test")
        };
        var handler = new UpdateUserCommandHandler(repository, new FakePasswordHasher());

        var result = await handler.Handle(
            new UpdateUserCommand(user.Id, "User", "taken@example.test", string.Empty, Role.User),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("User.EmailExists", result.Error.Code);
        Assert.False(repository.WasUpdated);
    }

    [Fact]
    public async Task Update_WhenUniqueConstraintWinsRace_ReturnsConflict()
    {
        var user = CreateUser("current@example.test");
        var repository = new FakeUserRepository
        {
            ExistingUser = user,
            ReturnConflictOnSave = true
        };
        var handler = new UpdateUserCommandHandler(repository, new FakePasswordHasher());

        var result = await handler.Handle(
            new UpdateUserCommand(user.Id, "User", "available@example.test", string.Empty, Role.User),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("User.EmailExists", result.Error.Code);
    }

    private static User CreateUser(string email) =>
        User.Create("User", email, "Password123", Role.User, new FakePasswordHasher()).Value;

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string plainTextPassword) => "hash";

        public bool Verify(string plainTextPassword, string hashedPassword) => true;

        public void PerformFakeVerification()
        {
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? ExistingUser { get; init; }

        public User? ExistingEmailUser { get; init; }

        public User? AddedUser { get; private set; }

        public bool ReturnConflictOnSave { get; init; }

        public bool WasUpdated { get; private set; }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(ExistingUser?.Id == id ? ExistingUser : null);

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(
                FindByEmail(ExistingEmailUser, email) ?? FindByEmail(ExistingUser, email));

        public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<User>>(ExistingUser is null ? [] : [ExistingUser]);

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            AddedUser = user;
            return Task.CompletedTask;
        }

        public void Update(User user)
        {
            WasUpdated = true;
        }

        public void Remove(User user)
        {
        }

        public Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (ReturnConflictOnSave)
                return Task.FromResult<Result<int>>(
                    Error.Conflict("User.EmailExists", "A user with that email already exists."));

            return Task.FromResult<Result<int>>(1);
        }

        private static User? FindByEmail(User? user, string email) =>
            user?.Email.Value == email.Trim().ToLowerInvariant() ? user : null;
    }
}
