using Microsoft.EntityFrameworkCore;
using SharedKernal.Seeding;

namespace Users.Infrastracture.Persistence;

internal sealed class UsersDatabaseMigrator(UsersDbContext db) : IDatabaseMigrator
{
    private const long AdvisoryLockId = 73420518;

    public int Order => 100;

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await db.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await db.Database.ExecuteSqlRawAsync(
                $"SELECT pg_advisory_lock({AdvisoryLockId})",
                cancellationToken);

            try
            {
                await db.Database.MigrateAsync(cancellationToken);
            }
            finally
            {
                await db.Database.ExecuteSqlRawAsync(
                    $"SELECT pg_advisory_unlock({AdvisoryLockId})",
                    CancellationToken.None);
            }
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
