using Microsoft.Extensions.DependencyInjection;

namespace Posts.Application;

public static class ApplicationExtension
{
    public static IServiceCollection AddPostsApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(ApplicationExtension).Assembly));
        return services;
    }
}
