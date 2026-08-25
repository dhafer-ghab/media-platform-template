using Microsoft.EntityFrameworkCore;
using Users.Infrastracture.Persistence;

namespace Users.Infrastracture.Seeding;

internal sealed class UserSeedStore(UsersDbContext db) : IUserSeedStore
{
    private const long AdvisoryLockId = 73420519;

    public async Task RunLockedAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        await db.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await db.Database.ExecuteSqlRawAsync(
                $"SELECT pg_advisory_lock({AdvisoryLockId})",
                cancellationToken);

            try
            {
                await action(cancellationToken);
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

    public Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return db.Users.AsNoTracking().AnyAsync(
            user => user.Email.Value == normalizedEmail,
            cancellationToken);
    }
}
