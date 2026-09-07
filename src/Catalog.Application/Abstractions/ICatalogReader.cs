using Catalog.Contracts;

namespace Catalog.Application.Abstractions;

public interface ICatalogReader
{
    Task<PagedResult<ActorSummary>> GetActorsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<ActorProfile?> GetActorAsync(string slug, CancellationToken cancellationToken);
    Task<PagedResult<AlbumSummary>?> GetActorAlbumsAsync(string actorSlug, int page, int pageSize, CancellationToken cancellationToken);
    Task<AlbumDetails?> GetAlbumAsync(string actorSlug, string albumSlug, CancellationToken cancellationToken);
    Task<PagedResult<PostDetails>?> GetAlbumPostsAsync(string actorSlug, string albumSlug, int page, int pageSize, CancellationToken cancellationToken);
    Task<PostDetails?> GetPostAsync(string slug, CancellationToken cancellationToken);
    Task<PostDetails?> GetPostByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<LegacyRouteResolution?> ResolveLegacyRouteAsync(string sourcePath, CancellationToken cancellationToken);
    Task<PagedResult<DiscoveryItem>> DiscoverAsync(string? query, string? tag, int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken cancellationToken);
}
