using SharedKernel.Entities.Enums;

namespace Users.Infrastracture.Seeding;

internal sealed class UserSeedOptions
{
    public const string SectionName = "UserSeed";

    public bool Enabled { get; init; }
    public List<UserSeedDefinition> Users { get; init; } = [];
}

internal sealed record UserSeedDefinition
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public Role Role { get; init; }
}
