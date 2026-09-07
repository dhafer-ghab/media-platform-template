namespace Host.WebApi.Configuration;

internal sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public bool? ApplyMigrations { get; init; }
}
