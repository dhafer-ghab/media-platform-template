using Microsoft.Extensions.DependencyInjection;
using Storage.Presentation.Authorization;

namespace Storage.Presentation;

public static class Extension
{
    public static IServiceCollection AddStoragePresentation(this IServiceCollection services)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(Extension).Assembly);

        services.AddStorageAuthorization();

        return services;
    }
}
