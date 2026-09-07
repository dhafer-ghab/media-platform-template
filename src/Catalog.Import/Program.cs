using Catalog.Infrastructure;
using Catalog.Infrastructure.Import;
using Microsoft.Extensions.DependencyInjection;

if (!TryReadArguments(args, out var file, out var dryRun, out var error))
{
    Console.Error.WriteLine(error);
    Console.Error.WriteLine("Usage: Catalog.Import --file <path> [--dry-run]");
    return 2;
}

var connection = Environment.GetEnvironmentVariable("CATALOG_CONNECTION_STRING")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__PostgreConnectionString");
if (string.IsNullOrWhiteSpace(connection))
{
    Console.Error.WriteLine("A connection is required via CATALOG_CONNECTION_STRING or ConnectionStrings__PostgreConnectionString.");
    return 2;
}

if (!File.Exists(file))
{
    Console.Error.WriteLine($"Import file '{file}' does not exist.");
    return 2;
}

try
{
    await using var services = new ServiceCollection().AddCatalogModule(connection).BuildServiceProvider();
    await using var scope = services.CreateAsyncScope();
    await using var stream = File.OpenRead(file);
    var result = await scope.ServiceProvider.GetRequiredService<CatalogJsonImporter>()
        .ImportAsync(stream, dryRun);
    Console.WriteLine($"Actors: {result.Actors}; albums: {result.Albums}; posts: {result.Posts}; legacy routes: {result.LegacyRoutes}; dry-run: {result.DryRun}.");
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

static bool TryReadArguments(
    string[] args,
    out string file,
    out bool dryRun,
    out string error)
{
    file = string.Empty;
    dryRun = false;
    error = string.Empty;

    for (var index = 0; index < args.Length; index++)
    {
        switch (args[index])
        {
            case "--file" when index + 1 < args.Length:
                file = args[++index];
                break;
            case "--dry-run":
                dryRun = true;
                break;
            default:
                error = $"Unknown or incomplete argument '{args[index]}'.";
                return false;
        }
    }

    if (string.IsNullOrWhiteSpace(file))
    {
        error = "--file is required.";
        return false;
    }

    return true;
}
