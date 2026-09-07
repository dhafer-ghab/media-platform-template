using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Application;

public static class CatalogApplicationExtensions
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(CatalogApplicationExtensions).Assembly));
        return services;
    }
}
