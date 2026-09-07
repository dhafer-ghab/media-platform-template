using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Presentation;

public static class CatalogPresentationExtensions
{
    public static IServiceCollection AddCatalogPresentation(this IServiceCollection services)
    {
        services.AddControllers().AddApplicationPart(typeof(CatalogPresentationExtensions).Assembly);
        return services;
    }
}
