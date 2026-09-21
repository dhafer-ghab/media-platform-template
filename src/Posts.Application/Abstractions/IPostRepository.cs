using Posts.Application.Posts;

namespace Posts.Application.Abstractions;

internal interface IPostRepository
{
    Task<PagedPostsDto> GetPublishedAsync(
        string? keyword,
        string? category,
        string? tag,
        PostSort sort,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<PostDto?> GetPublishedByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PostFacetsDto> GetPublishedFacetsAsync(CancellationToken cancellationToken);
    Task<bool> IncrementPublishedViewCountAsync(Guid id, CancellationToken cancellationToken);
}
