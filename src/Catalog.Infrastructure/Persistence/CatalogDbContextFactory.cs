using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Catalog.Infrastructure.Persistence;

internal sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CATALOG_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__PostgreConnectionString")
            ?? throw new InvalidOperationException(
                "CATALOG_CONNECTION_STRING or ConnectionStrings__PostgreConnectionString is required for design-time operations.");
        var options = new DbContextOptionsBuilder<CatalogDbContext>();
        CatalogModule.Configure(options, connectionString);
        return new CatalogDbContext(options.Options);
    }
}
