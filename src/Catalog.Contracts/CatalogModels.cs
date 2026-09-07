namespace Catalog.Contracts;

public sealed record ActorProfile(
    Guid Id,
    string Slug,
    string DisplayName,
    string Profession,
    string Bio,
    string AvatarStorageKey,
    long FollowerCount,
    int EditorialRank,
    DateTimeOffset PublishedAt,
    DateTimeOffset UpdatedAt,
    int AlbumCount,
    int MediaCount);

public sealed record ActorSummary(
    Guid Id,
    string Slug,
    string DisplayName,
    string Profession,
    string AvatarStorageKey,
    long FollowerCount,
    int EditorialRank,
    DateTimeOffset PublishedAt,
    DateTimeOffset UpdatedAt,
    int AlbumCount,
    int MediaCount);

public sealed record AlbumDetails(
    Guid Id,
    Guid ActorId,
    string ActorSlug,
    string ActorName,
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
    DateTimeOffset UpdatedAt,
    int PhotoCount,
    int VideoCount);

public sealed record AlbumSummary(
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
    DateTimeOffset UpdatedAt,
    int PhotoCount,
    int VideoCount);

public sealed record PostDetails(
    Guid Id,
    Guid AlbumId,
    Guid ActorId,
    string Slug,
    string StorageKey,
    string MediaType,
    int Width,
    int Height,
    double AspectRatio,
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

public sealed record DiscoveryItem(
    Guid Id,
    Guid ActorId,
    string ActorSlug,
    string ActorName,
    string Slug,
    string Title,
    string Description,
    string CoverStorageKey,
    string CoverAltText,
    double CoverAspectRatio,
    IReadOnlyList<string> Tags,
    int EditorialRank,
    DateTimeOffset PublishedAt,
    DateTimeOffset UpdatedAt);

public sealed record LegacyRouteResolution(
    Guid Id,
    string SourcePath,
    string DestinationPath,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasPrev,
    bool HasNext);

public static class CatalogPagination
{
    public const int DefaultPageSize = 24;
    public const int MaximumPageSize = 100;
    public const int MaximumPage = 1_000_000;
}
