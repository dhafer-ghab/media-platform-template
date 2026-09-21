using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities.Enums;
using Users.Domain;
using Users.Domain.Abstractions;
using Users.Infrastracture.Persistence;

namespace Users.Infrastracture.Seeding;

internal static class UserSeeder
{
    private static readonly HashSet<Role> RequiredRoles =
    [
        Role.Admin,
        Role.User,
        Role.PremiumUser
    ];

    internal static void Configure(
        DbContextOptionsBuilder options,
        UserSeedOptions seedOptions,
        IPasswordHasher passwordHasher)
    {
        options
            .UseSeeding((context, _) =>
                Seed((UsersDbContext)context, seedOptions, passwordHasher))
            .UseAsyncSeeding((context, _, cancellationToken) =>
                SeedAsync((UsersDbContext)context, seedOptions, passwordHasher, cancellationToken));
    }

    private static void Seed(
        UsersDbContext context,
        UserSeedOptions options,
        IPasswordHasher passwordHasher)
    {
        if (!options.Enabled)
            return;

        Validate(options.Users);

        var existingEmails = context.Users
            .Select(user => user.Email.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        AddMissingUsers(context, options.Users, existingEmails, passwordHasher);

        if (context.ChangeTracker.HasChanges())
            context.SaveChanges();
    }

    private static async Task SeedAsync(
        UsersDbContext context,
        UserSeedOptions options,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken)
    {
        if (!options.Enabled)
            return;

        Validate(options.Users);

        var existingEmails = (await context.Users
                .Select(user => user.Email.Value)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        AddMissingUsers(context, options.Users, existingEmails, passwordHasher);

        if (context.ChangeTracker.HasChanges())
            await context.SaveChangesAsync(cancellationToken);
    }

    private static void AddMissingUsers(
        UsersDbContext context,
        IEnumerable<UserSeedDefinition> definitions,
        ISet<string> existingEmails,
        IPasswordHasher passwordHasher)
    {
        foreach (var definition in definitions)
        {
            var normalizedEmail = definition.Email.Trim().ToLowerInvariant();
            if (!existingEmails.Add(normalizedEmail))
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

            result.Value.ClearDomainEvents();
            context.Users.Add(result.Value);
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
