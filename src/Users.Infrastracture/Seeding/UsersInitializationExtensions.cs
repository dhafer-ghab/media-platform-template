using Microsoft.Extensions.DependencyInjection;

namespace Users.Infrastracture.Seeding;

public static class UsersInitializationExtensions
{
    public static IServiceCollection AddUsersSeeding(this IServiceCollection services)
    {
        services.AddHostedService<UserSeedHostedService>();
        return services;
    }
}
