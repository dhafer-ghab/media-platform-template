namespace Catalog.Domain;

public sealed class LegacyRoute
{
    private LegacyRoute() { }

    public LegacyRoute(
        Guid id,
        string sourcePath,
        string destinationPath,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        CatalogValidation.Id(id, nameof(id));
        Id = id;
        Update(sourcePath, destinationPath, createdAt, updatedAt);
    }

    public Guid Id { get; private set; }
    public string SourcePath { get; private set; } = string.Empty;
    public string DestinationPath { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        string sourcePath,
        string destinationPath,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        var timestamps = CatalogValidation.Timestamps(createdAt, updatedAt);
        SourcePath = NormalizeSourcePath(sourcePath);
        DestinationPath = CatalogValidation.LocalRootedPath(destinationPath, 1000, nameof(destinationPath));
        CreatedAt = timestamps.PublishedAt;
        UpdatedAt = timestamps.UpdatedAt;
    }

    public static string NormalizeSourcePath(string sourcePath)
    {
        var candidate = sourcePath is { Length: > 1 } ? sourcePath.TrimEnd('/') : sourcePath;
        return CatalogValidation.LocalRootedPath(candidate, 1000, nameof(sourcePath)).ToLowerInvariant();
    }
}
