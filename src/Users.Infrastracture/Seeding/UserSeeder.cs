using Microsoft.Extensions.Options;
using SharedKernel.Entities.Enums;
using Users.Application.Abstractions;
using Users.Domain;
using Users.Domain.Abstractions;

namespace Users.Infrastracture.Seeding;

internal sealed class UserSeeder(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IOptions<UserSeedOptions> options)
{
    private static readonly HashSet<Role> RequiredRoles =
    [
        Role.Admin,
        Role.User,
        Role.PremiumUser
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var seedOptions = options.Value;
        if (!seedOptions.Enabled)
            return;

        Validate(seedOptions.Users);

        var usersAdded = false;

        foreach (var definition in seedOptions.Users)
        {
            var normalizedEmail = definition.Email.Trim().ToLowerInvariant();
            if (await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken) is not null)
                continue;

            var result = User.Create(
                definition.Name,
                normalizedEmail,
                definition.Password,
                definition.Role,
                passwordHasher);

            if (result.IsFailure)
            {
                var errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Message}"));
                throw new InvalidOperationException($"Invalid seed user '{normalizedEmail}': {errors}");
            }

            await userRepository.AddAsync(result.Value, cancellationToken);
            usersAdded = true;
        }

        if (!usersAdded)
            return;

        var saveResult = await userRepository.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            var errors = string.Join("; ", saveResult.Errors.Select(error => $"{error.Code}: {error.Message}"));
            throw new InvalidOperationException($"Could not save seeded users: {errors}");
        }
    }

    private static void Validate(IReadOnlyCollection<UserSeedDefinition> users)
    {
        if (users.Count != RequiredRoles.Count || !users.Select(user => user.Role).ToHashSet().SetEquals(RequiredRoles))
        {
            throw new InvalidOperationException(
                "UserSeed must contain exactly one Admin, one User, and one PremiumUser.");
        }

        var uniqueEmails = users
            .Select(user => user.Email.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (uniqueEmails.Count != users.Count)
            throw new InvalidOperationException("UserSeed email addresses must be unique.");
    }
}
