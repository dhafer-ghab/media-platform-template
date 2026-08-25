using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using SharedKernal.Results;
using Users.Application.Users.Commands.CreateUser;
using Users.Common;
using Users.Infrastracture.Seeding;

namespace Users.Infrastructure.UnitTests;

public sealed class UsersDataSeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesOnlyMissingUsers()
    {
        var store = new Mock<IUserSeedStore>();
        store.Setup(candidate => candidate.RunLockedAsync(
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((action, ct) => action(ct));
        store.Setup(candidate => candidate.ExistsAsync("admin@example.test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        store.Setup(candidate => candidate.ExistsAsync("user@example.test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sender = new Mock<ISender>();
        sender.Setup(candidate => candidate.Send(
                It.Is<CreateUserCommand>(command =>
                    command.Email == "user@example.test" &&
                    command.Password == "Password123" &&
                    command.Role == Role.User),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Guid.NewGuid()));

        var seeder = CreateSeeder(store.Object, sender.Object,
        [
            new SeedUserOptions { Name = "Admin", Email = "admin@example.test", Role = Role.Admin },
            new SeedUserOptions { Name = "User", Email = "user@example.test", Role = Role.User }
        ]);

        await seeder.SeedAsync();

        sender.Verify(candidate => candidate.Send(
            It.IsAny<CreateUserCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SeedAsync_WithDuplicateEmails_FailsBeforeAcquiringLock()
    {
        var store = new Mock<IUserSeedStore>();
        var seeder = CreateSeeder(store.Object, Mock.Of<ISender>(),
        [
            new SeedUserOptions { Name = "First", Email = "USER@example.test" },
            new SeedUserOptions { Name = "Second", Email = "user@example.test" }
        ]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => seeder.SeedAsync());

        Assert.Contains("configured more than once", exception.Message);
        store.Verify(candidate => candidate.RunLockedAsync(
            It.IsAny<Func<CancellationToken, Task>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SeedAsync_WithoutAnyPassword_FailsClearly()
    {
        var store = new Mock<IUserSeedStore>();
        var seeder = CreateSeeder(
            store.Object,
            Mock.Of<ISender>(),
            [new SeedUserOptions { Name = "User", Email = "user@example.test" }],
            defaultPassword: null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => seeder.SeedAsync());

        Assert.Contains("DefaultPassword", exception.Message);
    }

    [Fact]
    public async Task SeedAsync_WhenUserIsCreatedConcurrently_TreatsConflictAsSuccess()
    {
        var store = new Mock<IUserSeedStore>();
        store.Setup(candidate => candidate.RunLockedAsync(
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((action, ct) => action(ct));
        store.SetupSequence(candidate => candidate.ExistsAsync(
                "user@example.test",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        var sender = new Mock<ISender>();
        sender.Setup(candidate => candidate.Send(
                It.IsAny<CreateUserCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Guid>(
                Error.Conflict("User.EmailExists", "A user with that email already exists.")));
        var seeder = CreateSeeder(
            store.Object,
            sender.Object,
            [new SeedUserOptions { Name = "User", Email = "user@example.test" }]);

        await seeder.SeedAsync();

        store.Verify(candidate => candidate.ExistsAsync(
            "user@example.test",
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private static UsersDataSeeder CreateSeeder(
        IUserSeedStore store,
        ISender sender,
        List<SeedUserOptions> accounts,
        string? defaultPassword = "Password123")
    {
        var options = Options.Create(new UsersSeedOptions
        {
            DefaultPassword = defaultPassword,
            Accounts = accounts
        });

        return new UsersDataSeeder(
            store,
            sender,
            options,
            NullLogger<UsersDataSeeder>.Instance);
    }
}
