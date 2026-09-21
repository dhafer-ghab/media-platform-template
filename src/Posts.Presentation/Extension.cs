using Microsoft.Extensions.DependencyInjection;

namespace Posts.Presentation;

public static class Extension
{
    public static IServiceCollection AddPostsPresentation(this IServiceCollection services)
    {
        services.AddControllers().AddApplicationPart(typeof(Extension).Assembly);
        return services;
    }
}
