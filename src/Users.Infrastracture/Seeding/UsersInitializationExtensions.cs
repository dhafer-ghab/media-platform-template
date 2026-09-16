using Microsoft.Extensions.DependencyInjection;

namespace Users.Infrastracture.Seeding;

public static class UsersInitializationExtensions
{
    public static async Task SeedUsersAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<UserSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
