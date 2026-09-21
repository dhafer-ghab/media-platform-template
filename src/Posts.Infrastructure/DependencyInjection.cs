using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Posts.Application;
using Posts.Application.Abstractions;
using Posts.Infrastructure.Persistence;

namespace Posts.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPostsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreConnectionString")
            ?? throw new InvalidOperationException("Connection string 'PostgreConnectionString' is required.");

        services.AddDbContextPool<PostsDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__PostsMigrations", "Posts")));

        services.AddPostsApplication();
        services.AddScoped<IPostRepository, PostRepository>();
        return services;
    }
}
