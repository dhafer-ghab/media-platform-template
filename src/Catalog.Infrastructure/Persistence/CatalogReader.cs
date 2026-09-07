using System.Data;
using Catalog.Application.Abstractions;
using Catalog.Contracts;
using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

internal sealed class CatalogReader(CatalogDbContext context) : ICatalogReader
{
    public async Task<PagedResult<ActorSummary>> GetActorsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.RepeatableRead, cancellationToken);
        var actors = context.Actors.AsNoTracking();
        var totalItems = await actors.CountAsync(cancellationToken);
        var items = await actors
            .OrderByDescending(actor => actor.EditorialRank)
            .ThenByDescending(actor => actor.PublishedAt)
            .ThenBy(actor => actor.Id)
            .Skip(GetOffset(page, pageSize))
            .Take(pageSize)
            .Select(actor => new ActorSummary(
                actor.Id, actor.Slug, actor.DisplayName, actor.Profession,
                actor.AvatarStorageKey, actor.FollowerCount, actor.EditorialRank,
                actor.PublishedAt, actor.UpdatedAt,
                context.Albums.Count(album => album.ActorId == actor.Id),
                context.Posts.Count(post => post.Album.ActorId == actor.Id)))
            .ToListAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return Page(items, page, pageSize, totalItems);
    }

    public Task<ActorProfile?> GetActorAsync(string slug, CancellationToken cancellationToken) =>
        context.Actors.AsNoTracking()
            .Where(actor => actor.Slug == slug)
            .Select(actor => new ActorProfile(
                actor.Id, actor.Slug, actor.DisplayName, actor.Profession, actor.Bio,
                actor.AvatarStorageKey, actor.FollowerCount, actor.EditorialRank,
                actor.PublishedAt, actor.UpdatedAt,
                context.Albums.Count(album => album.ActorId == actor.Id),
                context.Posts.Count(post => post.Album.ActorId == actor.Id)))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<AlbumSummary>?> GetActorAlbumsAsync(
        string actorSlug,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.RepeatableRead, cancellationToken);
        var actorId = await context.Actors.AsNoTracking()
            .Where(actor => actor.Slug == actorSlug)
            .Select(actor => (Guid?)actor.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (actorId is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        var albums = context.Albums.AsNoTracking().Where(album => album.ActorId == actorId);
        var totalItems = await albums.CountAsync(cancellationToken);
        var items = await albums
            .OrderByDescending(album => album.EditorialRank)
            .ThenByDescending(album => album.PublishedAt)
            .ThenBy(album => album.Id)
            .Skip(GetOffset(page, pageSize))
            .Take(pageSize)
            .Select(album => new AlbumSummary(
                album.Id, album.ActorId, album.Slug, album.Title, album.Description,
                album.CoverStorageKey, album.CoverAltText, album.CoverAspectRatio,
                album.IsSeries, album.Tags, album.EditorialRank, album.PublishedAt,
                album.UpdatedAt,
                context.Posts.Count(post => post.AlbumId == album.Id && post.MediaType == MediaType.Photo),
                context.Posts.Count(post => post.AlbumId == album.Id && post.MediaType == MediaType.Video)))
            .ToListAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return Page(items, page, pageSize, totalItems);
    }

    public Task<AlbumDetails?> GetAlbumAsync(
        string actorSlug,
        string albumSlug,
        CancellationToken cancellationToken)
    => context.Albums.AsNoTracking()
            .Where(x => x.Actor.Slug == actorSlug && x.Slug == albumSlug)
            .Select(album => new AlbumDetails(
                album.Id, album.ActorId, album.Actor.Slug, album.Actor.DisplayName,
                album.Slug, album.Title, album.Description, album.CoverStorageKey,
                album.CoverAltText, album.CoverAspectRatio, album.IsSeries, album.Tags,
                album.EditorialRank, album.PublishedAt, album.UpdatedAt,
                context.Posts.Count(post => post.AlbumId == album.Id && post.MediaType == MediaType.Photo),
                context.Posts.Count(post => post.AlbumId == album.Id && post.MediaType == MediaType.Video)))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<PostDetails>?> GetAlbumPostsAsync(
        string actorSlug,
        string albumSlug,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.RepeatableRead, cancellationToken);
        var album = await context.Albums.AsNoTracking()
            .Where(item => item.Actor.Slug == actorSlug && item.Slug == albumSlug)
            .Select(item => new { item.Id, item.ActorId })
            .SingleOrDefaultAsync(cancellationToken);
        if (album is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        var posts = context.Posts.AsNoTracking().Where(post => post.AlbumId == album.Id);
        var totalItems = await posts.CountAsync(cancellationToken);
        var items = await posts
            .OrderBy(post => post.DisplayOrder)
            .ThenBy(post => post.Id)
            .Skip(GetOffset(page, pageSize))
            .Take(pageSize)
            .Select(post => ToPostDetails(post, album.ActorId))
            .ToListAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return Page(items, page, pageSize, totalItems);
    }

    public Task<PostDetails?> GetPostAsync(string slug, CancellationToken cancellationToken) =>
        context.Posts.AsNoTracking()
            .Where(post => post.Slug == slug)
            .Select(post => ToPostDetails(post, post.Album.ActorId))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<PostDetails?> GetPostByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Posts.AsNoTracking()
            .Where(post => post.Id == id)
            .Select(post => ToPostDetails(post, post.Album.ActorId))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<LegacyRouteResolution?> ResolveLegacyRouteAsync(
        string sourcePath,
        CancellationToken cancellationToken)
    {
        var normalizedPath = LegacyRoute.NormalizeSourcePath(sourcePath);
        return context.LegacyRoutes.AsNoTracking()
            .Where(route => route.SourcePath == normalizedPath)
            .Select(route => new LegacyRouteResolution(
                route.Id, route.SourcePath, route.DestinationPath,
                route.CreatedAt, route.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<DiscoveryItem>> DiscoverAsync(
        string? query,
        string? tag,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.RepeatableRead, cancellationToken);
        var albums = context.Albums.AsNoTracking().AsQueryable();
        if (query is not null)
            albums = albums.Where(album =>
                EF.Functions.ILike(album.Title, $"%{query}%") ||
                EF.Functions.ILike(album.Description, $"%{query}%") ||
                EF.Functions.ILike(album.Actor.DisplayName, $"%{query}%"));
        if (tag is not null)
            albums = albums.Where(album => album.Tags.Contains(tag) ||
                context.Posts.Any(post => post.AlbumId == album.Id && post.Tags.Contains(tag)));

        var totalItems = await albums.CountAsync(cancellationToken);
        var items = await albums
            .OrderByDescending(album => album.EditorialRank)
            .ThenByDescending(album => album.PublishedAt)
            .ThenBy(album => album.Id)
            .Skip(GetOffset(page, pageSize))
            .Take(pageSize)
            .Select(album => new DiscoveryItem(
                album.Id, album.ActorId, album.Actor.Slug, album.Actor.DisplayName,
                album.Slug, album.Title, album.Description, album.CoverStorageKey,
                album.CoverAltText, album.CoverAspectRatio, album.Tags,
                album.EditorialRank, album.PublishedAt, album.UpdatedAt))
            .ToListAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Page(items, page, pageSize, totalItems);
    }

    public async Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken cancellationToken)
    {
        return await context.Albums.AsNoTracking().SelectMany(album => album.Tags)
            .Concat(context.Posts.AsNoTracking().SelectMany(post => post.Tags))
            .Distinct()
            .OrderBy(tag => tag)
            .ToArrayAsync(cancellationToken);
    }

    private static PostDetails ToPostDetails(Post post, Guid actorId) => new(
        post.Id, post.AlbumId, actorId, post.Slug, post.StorageKey,
        post.MediaType == MediaType.Photo ? "photo" : "video", post.Width, post.Height,
        post.Height == 0 ? 0 : post.Width / (double)post.Height,
        post.DurationSeconds, post.MimeType, post.ByteSize, post.Caption,
        post.AltText, post.DisplayOrder, post.Tags, post.EditorialRank,
        post.PublishedAt, post.UpdatedAt);

    private static int GetOffset(int page, int pageSize)
    {
        if (page is < 1 or > CatalogPagination.MaximumPage)
            throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > CatalogPagination.MaximumPageSize)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        var offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(page), "The requested page offset is too large.");
        return (int)offset;
    }

    private static PagedResult<T> Page<T>(IReadOnlyList<T> items, int page, int pageSize, int totalItems)
    {
        var totalPages = totalItems == 0 ? 0 : (totalItems + (long)pageSize - 1) / pageSize;
        return new PagedResult<T>(
            items, page, pageSize, totalItems, checked((int)totalPages),
            totalPages > 0 && page > 1, page < totalPages);
    }
}
