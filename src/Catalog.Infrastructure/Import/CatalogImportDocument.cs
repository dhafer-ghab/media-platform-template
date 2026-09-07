using Catalog.Domain;

namespace Catalog.Infrastructure.Import;

public sealed record CatalogImportDocument(
    IReadOnlyList<ActorImportItem> Actors,
    IReadOnlyList<AlbumImportItem> Albums,
    IReadOnlyList<PostImportItem> Posts,
    IReadOnlyList<LegacyRouteImportItem> LegacyRoutes);

public sealed record ActorImportItem(
    Guid Id,
    string Slug,
    string DisplayName,
    string Profession,
    string Bio,
    string AvatarStorageKey,
    long FollowerCount,
    int EditorialRank,
    DateTimeOffset PublishedAt,
    DateTimeOffset UpdatedAt);

public sealed record AlbumImportItem(
    Guid Id,
    Guid ActorId,
    string Slug,
    string Title,
    string Description,
    string CoverStorageKey,
    string CoverAltText,
    double CoverAspectRatio,
    bool IsSeries,
    IReadOnlyList<string> Tags,
    int EditorialRank,
    DateTimeOffset PublishedAt,
    DateTimeOffset UpdatedAt);

public sealed record PostImportItem(
    Guid Id,
    Guid AlbumId,
    string Slug,
    string StorageKey,
    MediaType MediaType,
    int Width,
    int Height,
    int? DurationSeconds,
    string MimeType,
    long ByteSize,
    string? Caption,
    string AltText,
    int DisplayOrder,
    IReadOnlyList<string> Tags,
    int EditorialRank,
    DateTimeOffset PublishedAt,
    DateTimeOffset UpdatedAt);

public sealed record LegacyRouteImportItem(
    Guid Id,
    string SourcePath,
    string DestinationPath,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CatalogImportResult(int Actors, int Albums, int Posts, int LegacyRoutes, bool DryRun);
