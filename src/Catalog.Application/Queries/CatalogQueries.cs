using Catalog.Application.Abstractions;
using Catalog.Contracts;
using Catalog.Domain;
using SharedKernal.Messaging;
using SharedKernal.Results;

namespace Catalog.Application.Queries;

public sealed record GetActorsQuery(int Page = 1, int PageSize = CatalogPagination.DefaultPageSize)
    : IQuery<Result<PagedResult<ActorSummary>>>;
public sealed record GetActorProfileQuery(string Slug) : IQuery<Result<ActorProfile>>;
public sealed record GetActorAlbumsQuery(string ActorSlug, int Page = 1, int PageSize = CatalogPagination.DefaultPageSize)
    : IQuery<Result<PagedResult<AlbumSummary>>>;
public sealed record GetAlbumQuery(string ActorSlug, string AlbumSlug) : IQuery<Result<AlbumDetails>>;
public sealed record GetAlbumPostsQuery(
    string ActorSlug,
    string AlbumSlug,
    int Page = 1,
    int PageSize = CatalogPagination.DefaultPageSize) : IQuery<Result<PagedResult<PostDetails>>>;
public sealed record GetPostQuery(string Slug) : IQuery<Result<PostDetails>>;
public sealed record GetPostByIdQuery(Guid Id) : IQuery<Result<PostDetails>>;
public sealed record ResolveLegacyRouteQuery(string Path) : IQuery<Result<LegacyRouteResolution>>;
public sealed record DiscoverCatalogQuery(string? Query, string? Tag, int Page = 1, int PageSize = CatalogPagination.DefaultPageSize)
    : IQuery<Result<PagedResult<DiscoveryItem>>>;
public sealed record GetCatalogTagsQuery : IQuery<Result<IReadOnlyList<string>>>;

internal sealed class GetActorsQueryHandler(ICatalogReader reader)
    : IQueryHandler<GetActorsQuery, Result<PagedResult<ActorSummary>>>
{
    public async Task<Result<PagedResult<ActorSummary>>> Handle(
        GetActorsQuery request,
        CancellationToken cancellationToken)
    {
        var paginationError = PaginationValidation.Validate(request.Page, request.PageSize);
        if (paginationError is not null)
            return paginationError;

        return await reader.GetActorsAsync(request.Page, request.PageSize, cancellationToken);
    }
}

internal sealed class GetActorProfileQueryHandler(ICatalogReader reader)
    : IQueryHandler<GetActorProfileQuery, Result<ActorProfile>>
{
    public async Task<Result<ActorProfile>> Handle(GetActorProfileQuery request, CancellationToken cancellationToken)
    {
        var actor = await reader.GetActorAsync(request.Slug, cancellationToken);
        return actor is null
            ? Error.NotFound("Catalog.ActorNotFound", "Actor was not found.")
            : actor;
    }
}

internal sealed class GetActorAlbumsQueryHandler(ICatalogReader reader)
    : IQueryHandler<GetActorAlbumsQuery, Result<PagedResult<AlbumSummary>>>
{
    public async Task<Result<PagedResult<AlbumSummary>>> Handle(
        GetActorAlbumsQuery request,
        CancellationToken cancellationToken)
    {
        var paginationError = PaginationValidation.Validate(request.Page, request.PageSize);
        if (paginationError is not null)
            return paginationError;

        var albums = await reader.GetActorAlbumsAsync(
            request.ActorSlug, request.Page, request.PageSize, cancellationToken);
        return albums is null
            ? Error.NotFound("Catalog.ActorNotFound", "Actor was not found.")
            : albums;
    }
}

internal sealed class GetAlbumQueryHandler(ICatalogReader reader)
    : IQueryHandler<GetAlbumQuery, Result<AlbumDetails>>
{
    public async Task<Result<AlbumDetails>> Handle(GetAlbumQuery request, CancellationToken cancellationToken)
    {
        var album = await reader.GetAlbumAsync(request.ActorSlug, request.AlbumSlug, cancellationToken);
        return album is null
            ? Error.NotFound("Catalog.AlbumNotFound", "Album was not found.")
            : album;
    }
}

internal sealed class GetAlbumPostsQueryHandler(ICatalogReader reader)
    : IQueryHandler<GetAlbumPostsQuery, Result<PagedResult<PostDetails>>>
{
    public async Task<Result<PagedResult<PostDetails>>> Handle(
        GetAlbumPostsQuery request,
        CancellationToken cancellationToken)
    {
        var paginationError = PaginationValidation.Validate(request.Page, request.PageSize);
        if (paginationError is not null)
            return paginationError;

        var posts = await reader.GetAlbumPostsAsync(
            request.ActorSlug, request.AlbumSlug, request.Page, request.PageSize, cancellationToken);
        return posts is null
            ? Error.NotFound("Catalog.AlbumNotFound", "Album was not found.")
            : posts;
    }
}

internal sealed class GetPostQueryHandler(ICatalogReader reader)
    : IQueryHandler<GetPostQuery, Result<PostDetails>>
{
    public async Task<Result<PostDetails>> Handle(GetPostQuery request, CancellationToken cancellationToken)
    {
        var post = await reader.GetPostAsync(request.Slug, cancellationToken);
        return post is null
            ? Error.NotFound("Catalog.PostNotFound", "Post was not found.")
            : post;
    }
}

internal sealed class GetPostByIdQueryHandler(ICatalogReader reader)
    : IQueryHandler<GetPostByIdQuery, Result<PostDetails>>
{
    public async Task<Result<PostDetails>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            return Error.Validation("Catalog.InvalidPostId", "Post ID cannot be empty.");

        var post = await reader.GetPostByIdAsync(request.Id, cancellationToken);
        return post is null
            ? Error.NotFound("Catalog.PostNotFound", "Post was not found.")
            : post;
    }
}

internal sealed class ResolveLegacyRouteQueryHandler(ICatalogReader reader)
    : IQueryHandler<ResolveLegacyRouteQuery, Result<LegacyRouteResolution>>
{
    public async Task<Result<LegacyRouteResolution>> Handle(
        ResolveLegacyRouteQuery request,
        CancellationToken cancellationToken)
    {
        string sourcePath;
        try
        {
            sourcePath = LegacyRoute.NormalizeSourcePath(request.Path);
        }
        catch (ArgumentException exception)
        {
            return Error.Validation("Catalog.InvalidLegacyPath", exception.Message);
        }

        var route = await reader.ResolveLegacyRouteAsync(sourcePath, cancellationToken);
        return route is null
            ? Error.NotFound("Catalog.LegacyRouteNotFound", "Legacy route was not found.")
            : route;
    }
}

internal sealed class DiscoverCatalogQueryHandler(ICatalogReader reader)
    : IQueryHandler<DiscoverCatalogQuery, Result<PagedResult<DiscoveryItem>>>
{
    public async Task<Result<PagedResult<DiscoveryItem>>> Handle(
        DiscoverCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var paginationError = PaginationValidation.Validate(request.Page, request.PageSize);
        if (paginationError is not null)
            return paginationError;

        return await reader.DiscoverAsync(
            Normalize(request.Query), Normalize(request.Tag), request.Page, request.PageSize, cancellationToken);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}

file static class PaginationValidation
{
    internal static Error? Validate(int page, int pageSize) =>
        page is < 1 or > CatalogPagination.MaximumPage ||
        pageSize is < 1 or > CatalogPagination.MaximumPageSize
            ? Error.Validation(
                "Catalog.InvalidPage",
                $"Page must be between 1 and {CatalogPagination.MaximumPage} and pageSize must be between 1 and {CatalogPagination.MaximumPageSize}.")
            : null;
}

internal sealed class GetCatalogTagsQueryHandler(ICatalogReader reader)
    : IQueryHandler<GetCatalogTagsQuery, Result<IReadOnlyList<string>>>
{
    public async Task<Result<IReadOnlyList<string>>> Handle(
        GetCatalogTagsQuery request,
        CancellationToken cancellationToken) =>
        Result.Success(await reader.GetTagsAsync(cancellationToken));
}
