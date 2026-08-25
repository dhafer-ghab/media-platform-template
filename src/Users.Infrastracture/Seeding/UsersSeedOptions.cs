using Users.Common;

namespace Users.Infrastracture.Seeding;

internal sealed class UsersSeedOptions
{
    public const string SectionName = "SeedData:Users";

    public string? DefaultPassword { get; init; }

    public List<SeedUserOptions> Accounts { get; init; } = [];
}

internal sealed class SeedUserOptions
{
    public string Name { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public Role Role { get; init; } = Role.User;
}
