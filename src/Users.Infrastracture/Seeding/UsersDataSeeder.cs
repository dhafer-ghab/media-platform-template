using MediatR;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernal.Seeding;
using Users.Application.Users.Commands.CreateUser;

namespace Users.Infrastracture.Seeding;

internal sealed class UsersDataSeeder(
    IUserSeedStore store,
    ISender sender,
    IOptions<UsersSeedOptions> options,
    ILogger<UsersDataSeeder> logger) : IDataSeeder
{
    public int Order => 100;

    public Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var accounts = options.Value.Accounts;
        Validate(accounts, options.Value.DefaultPassword);

        return store.RunLockedAsync(async ct =>
        {
            foreach (var account in accounts)
            {
                if (await store.ExistsAsync(account.Email, ct))
                {
                    logger.LogInformation("Seed user {Email} already exists", account.Email);
                    continue;
                }

                var result = await sender.Send(
                    new CreateUserCommand(
                        account.Name,
                        account.Email,
                        options.Value.DefaultPassword!,
                        account.Role),
                    ct);

                if (result.IsFailure)
                {
                    if (result.Error.Code == "User.EmailExists" &&
                        await store.ExistsAsync(account.Email, ct))
                    {
                        logger.LogInformation("Seed user {Email} was created concurrently", account.Email);
                        continue;
                    }

                    throw new InvalidOperationException(
                        $"Could not seed user '{account.Email}': " +
                        string.Join("; ", result.Errors.Select(error => error.Message)));
                }

                logger.LogInformation(
                    "Created seed user {Email} with role {Role}",
                    account.Email,
                    account.Role);
            }
        }, cancellationToken);
    }

    private static void Validate(IReadOnlyCollection<SeedUserOptions> accounts, string? defaultPassword)
    {
        if (accounts.Count == 0)
            return;

        if (string.IsNullOrWhiteSpace(defaultPassword))
            throw new InvalidOperationException(
                "SeedData:Users:DefaultPassword must be supplied through secrets or the environment.");

        if (defaultPassword.Length < 8 ||
            !defaultPassword.Any(char.IsUpper) ||
            !defaultPassword.Any(char.IsDigit))
        {
            throw new InvalidOperationException(
                "SeedData:Users:DefaultPassword must be at least 8 characters and contain an uppercase letter and a digit.");
        }

        var duplicateEmail = accounts
            .Where(account => !string.IsNullOrWhiteSpace(account.Email))
            .GroupBy(account => account.Email.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1)?.Key;

        if (duplicateEmail is not null)
            throw new InvalidOperationException($"Seed user email '{duplicateEmail}' is configured more than once.");

        foreach (var account in accounts)
        {
            if (string.IsNullOrWhiteSpace(account.Name) || string.IsNullOrWhiteSpace(account.Email))
                throw new InvalidOperationException("Each seed user requires a name and email.");

            if (account.Name.Trim().Length > 200)
                throw new InvalidOperationException(
                    $"Seed user '{account.Email}' has a name longer than 200 characters.");

            if (account.Email.Trim().Length > 256 ||
                !new EmailAddressAttribute().IsValid(account.Email.Trim()))
            {
                throw new InvalidOperationException($"Seed user email '{account.Email}' is invalid.");
            }

            if (!Enum.IsDefined(account.Role))
                throw new InvalidOperationException($"Seed user '{account.Email}' has an invalid role.");
        }
    }
}
