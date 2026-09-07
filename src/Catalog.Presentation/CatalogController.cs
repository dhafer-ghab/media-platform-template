using Catalog.Application.Queries;
using Catalog.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernal.Extensions;

namespace Catalog.Presentation;

[ApiController]
[AllowAnonymous]
[Route("api/catalog")]
public sealed class CatalogController(ISender sender) : ControllerBase
{
    [HttpGet("actors")]
    public async Task<IActionResult> GetActors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = CatalogPagination.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetActorsQuery(page, pageSize), cancellationToken);
        return result.Match(Ok);
    }

    [HttpGet("actors/{slug}")]
    public async Task<IActionResult> GetActor(string slug, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetActorProfileQuery(slug), cancellationToken);
        return result.Match(Ok);
    }

    [HttpGet("actors/{actorSlug}/albums")]
    public async Task<IActionResult> GetActorAlbums(
        string actorSlug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = CatalogPagination.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetActorAlbumsQuery(actorSlug, page, pageSize), cancellationToken);
        return result.Match(Ok);
    }

    [HttpGet("actors/{actorSlug}/albums/{albumSlug}")]
    public async Task<IActionResult> GetAlbum(
        string actorSlug,
        string albumSlug,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAlbumQuery(actorSlug, albumSlug), cancellationToken);
        return result.Match(Ok);
    }

    [HttpGet("actors/{actorSlug}/albums/{albumSlug}/posts")]
    public async Task<IActionResult> GetAlbumPosts(
        string actorSlug,
        string albumSlug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = CatalogPagination.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetAlbumPostsQuery(actorSlug, albumSlug, page, pageSize), cancellationToken);
        return result.Match(Ok);
    }

    [HttpGet("posts/by-id/{id:guid}")]
    public async Task<IActionResult> GetPostById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPostByIdQuery(id), cancellationToken);
        return result.Match(Ok);
    }

    [HttpGet("posts/{slug}")]
    public async Task<IActionResult> GetPost(string slug, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPostQuery(slug), cancellationToken);
        return result.Match(Ok);
    }

    [HttpGet("legacy-routes/resolve")]
    public async Task<IActionResult> ResolveLegacyRoute(
        [FromQuery] string path,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResolveLegacyRouteQuery(path), cancellationToken);
        return result.Match(Ok);
    }

    [HttpGet("discovery")]
    public async Task<IActionResult> Discover(
        [FromQuery(Name = "q")] string? query,
        [FromQuery] string? tag,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = CatalogPagination.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new DiscoverCatalogQuery(query, tag, page, pageSize), cancellationToken);
        return result.Match(Ok);
    }

    [HttpGet("tags")]
    public async Task<IActionResult> GetTags(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCatalogTagsQuery(), cancellationToken);
        return result.Match(Ok);
    }
}
