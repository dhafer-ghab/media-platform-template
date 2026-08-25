namespace SharedKernal.Seeding;

public interface IDatabaseMigrator
{
    int Order { get; }

    Task MigrateAsync(CancellationToken cancellationToken = default);
}
