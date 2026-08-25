namespace Users.Infrastracture.Seeding;

internal interface IUserSeedStore
{
    Task RunLockedAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);
}
