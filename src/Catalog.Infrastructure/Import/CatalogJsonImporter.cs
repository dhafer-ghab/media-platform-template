using System.Text.Json;
using System.Text.Json.Serialization;
using Catalog.Domain;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Import;

public sealed class CatalogJsonImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        RespectRequiredConstructorParameters = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    private readonly CatalogDbContext context;

    internal CatalogJsonImporter(CatalogDbContext context) => this.context = context;

    public async Task<CatalogImportResult> ImportAsync(
        Stream json,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        CatalogImportDocument document;
        try
        {
            document = await JsonSerializer.DeserializeAsync<CatalogImportDocument>(
                json, JsonOptions, cancellationToken)
                ?? throw new InvalidDataException("The catalog import document is empty.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"Invalid catalog JSON at {exception.Path ?? "the document root"}: {exception.Message}", exception);
        }

        Validate(document);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(1982746351)", cancellationToken);

        // A document is authoritative. Replacing in dependency order also handles
        // changed unique slugs and display orders without transient collisions.
        await context.LegacyRoutes.ExecuteDeleteAsync(cancellationToken);
        await context.Posts.ExecuteDeleteAsync(cancellationToken);
        await context.Albums.ExecuteDeleteAsync(cancellationToken);
        await context.Actors.ExecuteDeleteAsync(cancellationToken);

        foreach (var item in document.Actors)
        {
            context.Actors.Add(new Actor(
                item.Id, item.Slug, item.DisplayName, item.Profession, item.Bio,
                item.AvatarStorageKey, item.FollowerCount, item.EditorialRank,
                item.PublishedAt, item.UpdatedAt));
        }
        await context.SaveChangesAsync(cancellationToken);

        foreach (var item in document.Albums)
        {
            context.Albums.Add(new Album(
                item.Id, item.ActorId, item.Slug, item.Title, item.Description,
                item.CoverStorageKey, item.CoverAltText, item.CoverAspectRatio,
                item.IsSeries, item.Tags.ToArray(), item.EditorialRank, item.PublishedAt, item.UpdatedAt));
        }
        await context.SaveChangesAsync(cancellationToken);

        foreach (var item in document.Posts)
        {
            context.Posts.Add(new Post(
                item.Id, item.AlbumId, item.Slug, item.StorageKey, item.MediaType,
                item.Width, item.Height, item.DurationSeconds, item.MimeType,
                item.ByteSize, item.Caption, item.AltText, item.DisplayOrder,
                item.Tags.ToArray(), item.EditorialRank, item.PublishedAt, item.UpdatedAt));
        }
        await context.SaveChangesAsync(cancellationToken);

        foreach (var item in document.LegacyRoutes)
        {
            context.LegacyRoutes.Add(new LegacyRoute(
                item.Id, item.SourcePath, item.DestinationPath,
                item.CreatedAt, item.UpdatedAt));
        }
        await context.SaveChangesAsync(cancellationToken);

        if (dryRun)
            await transaction.RollbackAsync(cancellationToken);
        else
            await transaction.CommitAsync(cancellationToken);

        return new CatalogImportResult(
            document.Actors.Count, document.Albums.Count, document.Posts.Count,
            document.LegacyRoutes.Count, dryRun);
    }

    private static void Validate(CatalogImportDocument document)
    {
        if (document.Actors is null || document.Albums is null || document.Posts is null || document.LegacyRoutes is null)
            throw new InvalidDataException("The import document must contain actors, albums, posts, and legacyRoutes arrays.");

        EnsureUnique(document.Actors.Select(x => x.Id), "actor IDs");
        EnsureUnique(document.Actors.Select(x => x.Slug), "actor slugs");
        EnsureUnique(document.Albums.Select(x => x.Id), "album IDs");
        EnsureUnique(document.Albums.Select(x => (x.ActorId, x.Slug)), "album actorId/slug pairs");
        EnsureUnique(document.Posts.Select(x => x.Id), "post IDs");
        EnsureUnique(document.Posts.Select(x => x.Slug), "post slugs");
        EnsureUnique(document.Posts.Select(x => (x.AlbumId, x.DisplayOrder)), "post albumId/displayOrder pairs");
        EnsureUnique(document.LegacyRoutes.Select(x => x.Id), "legacy route IDs");

        string NormalizeLegacySource(LegacyRouteImportItem route)
        {
            try
            {
                return LegacyRoute.NormalizeSourcePath(route.SourcePath);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidDataException($"LegacyRoute '{route.Id}' is invalid: {exception.Message}", exception);
            }
        }

        EnsureUnique(document.LegacyRoutes.Select(NormalizeLegacySource), "normalized legacy route source paths");

        var actorIds = document.Actors.Select(x => x.Id).ToHashSet();
        var albumIds = document.Albums.Select(x => x.Id).ToHashSet();
        foreach (var album in document.Albums.Where(album => !actorIds.Contains(album.ActorId)))
            throw new InvalidDataException(
                $"Album '{album.Id}' references actor '{album.ActorId}', which is absent from the import snapshot.");
        foreach (var post in document.Posts.Where(post => !albumIds.Contains(post.AlbumId)))
            throw new InvalidDataException(
                $"Post '{post.Id}' references album '{post.AlbumId}', which is absent from the import snapshot.");

        foreach (var actor in document.Actors)
            ValidateEntity("Actor", actor.Id, () => new Actor(
                actor.Id, actor.Slug, actor.DisplayName, actor.Profession, actor.Bio,
                actor.AvatarStorageKey, actor.FollowerCount, actor.EditorialRank,
                actor.PublishedAt, actor.UpdatedAt));

        foreach (var album in document.Albums)
            ValidateEntity("Album", album.Id, () => new Album(
                album.Id, album.ActorId, album.Slug, album.Title, album.Description,
                album.CoverStorageKey, album.CoverAltText, album.CoverAspectRatio,
                album.IsSeries, album.Tags?.ToArray()!, album.EditorialRank,
                album.PublishedAt, album.UpdatedAt));

        foreach (var post in document.Posts)
            ValidateEntity("Post", post.Id, () => new Post(
                post.Id, post.AlbumId, post.Slug, post.StorageKey, post.MediaType,
                post.Width, post.Height, post.DurationSeconds, post.MimeType,
                post.ByteSize, post.Caption, post.AltText, post.DisplayOrder,
                post.Tags?.ToArray()!, post.EditorialRank, post.PublishedAt, post.UpdatedAt));

        foreach (var route in document.LegacyRoutes)
            ValidateEntity("LegacyRoute", route.Id, () => new LegacyRoute(
                route.Id, route.SourcePath, route.DestinationPath,
                route.CreatedAt, route.UpdatedAt));

        ValidateLegacyRedirects(document.LegacyRoutes);
    }

    private static void ValidateLegacyRedirects(IReadOnlyList<LegacyRouteImportItem> routes)
    {
        var redirects = routes.ToDictionary(
            route => LegacyRoute.NormalizeSourcePath(route.SourcePath),
            route => LegacyRoute.NormalizeSourcePath(route.DestinationPath),
            StringComparer.Ordinal);

        foreach (var (source, destination) in redirects)
        {
            if (source == destination)
                throw new InvalidDataException($"Legacy route '{source}' cannot redirect to itself.");
        }

        var states = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var source in redirects.Keys)
        {
            if (states.ContainsKey(source))
                continue;

            var path = new List<string>();
            var current = source;
            while (redirects.TryGetValue(current, out var destination))
            {
                if (states.TryGetValue(current, out var complete))
                {
                    if (!complete)
                        throw new InvalidDataException($"The import contains a legacy redirect cycle involving '{current}'.");
                    break;
                }

                states[current] = false;
                path.Add(current);
                current = destination;
            }

            foreach (var item in path)
                states[item] = true;
        }
    }

    private static void ValidateEntity(string entity, Guid id, Func<object> create)
    {
        try
        {
            _ = create();
        }
        catch (Exception exception) when (exception is ArgumentException or NullReferenceException)
        {
            throw new InvalidDataException($"{entity} '{id}' is invalid: {exception.Message}", exception);
        }
    }

    private static void EnsureUnique<T>(IEnumerable<T> values, string field) where T : notnull
    {
        var duplicate = values.GroupBy(value => value).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
            throw new InvalidDataException($"The import contains duplicate {field}: '{duplicate.Key}'.");
    }
}
