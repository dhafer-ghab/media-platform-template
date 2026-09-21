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
        IConfiguration configuration,
        bool enableSeeding = false)
    {
        services.AddDbContext(configuration, enableSeeding);

        services.AddUsersApplication(configuration);

        return services;
    }

    private static IServiceCollection AddDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        bool enableSeeding)
    {
        var connectionString = configuration.GetConnectionString("PostgreConnectionString");
        var seedOptions = configuration
            .GetSection(UserSeedOptions.SectionName)
            .Get<UserSeedOptions>() ?? new UserSeedOptions();
        var passwordHasher = new PasswordHasher();

        services.AddDbContextPool<UsersDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__UsersMigrations", "Users"));

            if (enableSeeding)
                UserSeeder.Configure(options, seedOptions, passwordHasher);
        });

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
        return services;
    }
}
