using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernal.Messaging;
using Users.Application;
using Users.Application.Abstractions;
using Users.Application.Messaging;
using Users.Domain.Abstractions;
using Users.Infrastracture.Persistence;
using Users.Infrastracture.Seeding;
using Users.Infrastracture.Security;

namespace Users.Infrastracture;

internal static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext(configuration);

        services.AddUsersApplication(configuration);

        return services;
    }

    private static IServiceCollection AddDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreConnectionString");

        services.AddDbContextPool<UsersDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__UsersMigrations", "Users")));

        return services;
    }

    private static IServiceCollection AddUsersApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.InitializeApplication();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<ITokenService, TokenService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        services.Configure<UserSeedOptions>(configuration.GetSection(UserSeedOptions.SectionName));
        services.AddScoped<UserSeeder>();

        return services;
    }
}
