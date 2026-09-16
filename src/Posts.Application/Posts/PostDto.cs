namespace Posts.Application.Posts;

public sealed record PostDto(
    Guid Id,
    Guid AuthorId,
    Guid MediaAssetId,
    string Title,
    string Description,
    string MediaUrl,
    string AltText,
    string Category,
    IReadOnlyList<string> Tags,
    string Status,
    DateTime CreatedDate,
    DateTime? UpdateDate,
    DateTime? PublishedAt,
    long ViewCount);

public sealed record PagedPostsDto(
    IReadOnlyList<PostDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record PostFacetDto(string Name, int Count);

public sealed record PostFacetsDto(
    IReadOnlyList<PostFacetDto> Categories,
    IReadOnlyList<PostFacetDto> Tags);
