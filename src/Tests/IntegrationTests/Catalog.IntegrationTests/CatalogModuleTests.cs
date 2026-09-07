using System.Data;
using System.Data.Common;
using System.Text;
using Catalog.Application.Abstractions;
using Catalog.Application.Queries;
using Catalog.Contracts;
using Catalog.Domain;
using Catalog.Infrastructure;
using Catalog.Infrastructure.Import;
using Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Catalog.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class CatalogModuleTests(PostgreSqlFixture fixture)
{
    private const string ActorId = "11111111-1111-1111-1111-111111111111";
    private const string AlbumId = "22222222-2222-2222-2222-222222222222";
    private const string PostId = "33333333-3333-3333-3333-333333333333";
    private const string LegacyRouteId = "44444444-4444-4444-4444-444444444444";

    [Fact]
    public async Task Migration_CreatesCatalogSchemaAndUniqueSlugIndexes()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM pg_indexes
            WHERE schemaname = 'Catalog'
              AND indexname IN ('UX_Actors_Slug', 'UX_Albums_ActorId_Slug', 'UX_Posts_Slug', 'UX_LegacyRoutes_SourcePath');
            """;
        await context.Database.OpenConnectionAsync();

        Assert.Equal(4, Convert.ToInt32(await command.ExecuteScalarAsync()));

        command.CommandText = """
            SELECT COUNT(*)
            FROM pg_constraint
            WHERE connamespace = '"Catalog"'::regnamespace
              AND conname IN (
                'CK_Actors_AvatarStorageKey', 'CK_Actors_Counts', 'CK_Actors_Id', 'CK_Actors_RequiredText', 'CK_Actors_Slug', 'CK_Actors_Timestamps',
                'CK_Albums_CoverStorageKey', 'CK_Albums_Ids', 'CK_Albums_RequiredText', 'CK_Albums_Slug', 'CK_Albums_Timestamps', 'CK_Albums_Values',
                'CK_Posts_Ids', 'CK_Posts_Media', 'CK_Posts_RequiredText', 'CK_Posts_Slug', 'CK_Posts_StorageKey', 'CK_Posts_Timestamps', 'CK_Posts_Values',
                'CK_LegacyRoutes_Id', 'CK_LegacyRoutes_SourcePath', 'CK_LegacyRoutes_DestinationPath', 'CK_LegacyRoutes_Timestamps');
            """;
        Assert.Equal(23, Convert.ToInt32(await command.ExecuteScalarAsync()));
    }

    [Fact]
    public async Task Import_IsIdempotent_AndQueriesReturnImportedCatalog()
    {
        await ImportAsync(Document("First title"));
        await ImportAsync(Document("Updated title"));

        await using var scope = fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        Assert.Equal(1, await context.Actors.CountAsync());
        Assert.Equal(1, await context.Albums.CountAsync());
        Assert.Equal(1, await context.Posts.CountAsync());
        Assert.Equal(1, await context.LegacyRoutes.CountAsync());

        var reader = scope.ServiceProvider.GetRequiredService<ICatalogReader>();
        var actor = await reader.GetActorAsync("ada-lovelace", default);
        var album = await reader.GetAlbumAsync("ada-lovelace", "analytical-engine", default);
        var post = await reader.GetPostAsync("engine-diagram", default);
        var discovery = await reader.DiscoverAsync("updated", "history", 1, 10, default);
        var tags = await reader.GetTagsAsync(default);

        Assert.NotNull(actor);
        Assert.Equal(1, actor.AlbumCount);
        Assert.Equal(1, actor.MediaCount);
        Assert.NotNull(album);
        Assert.Equal("Updated title", album.Title);
        Assert.Equal(1, album.PhotoCount);
        Assert.NotNull(post);
        Assert.Equal(1.5, post.AspectRatio);
        Assert.Equal("photo", post.MediaType);
        Assert.Single(discovery.Items);
        Assert.Contains("history", tags);
    }

    [Fact]
    public async Task DryRun_RollsBackChanges()
    {
        await ImportAsync(Document("Before dry run"));
        await ImportAsync(EmptyDocument, dryRun: true);

        await using var scope = fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        Assert.True(await context.Actors.AnyAsync(x => x.Id == Guid.Parse(ActorId)));
        Assert.True(await context.Albums.AnyAsync(x => x.Id == Guid.Parse(AlbumId)));
        Assert.True(await context.Posts.AnyAsync(x => x.Id == Guid.Parse(PostId)));
        Assert.True(await context.LegacyRoutes.AnyAsync(x => x.Id == Guid.Parse(LegacyRouteId)));
    }

    [Fact]
    public async Task Import_DeletesRowsAbsentFromSnapshot()
    {
        await ImportAsync(Document("Initial"));
        await ImportAsync(EmptyDocument);

        await using var scope = fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        Assert.Empty(await context.Actors.ToListAsync());
        Assert.Empty(await context.Albums.ToListAsync());
        Assert.Empty(await context.Posts.ToListAsync());
        Assert.Empty(await context.LegacyRoutes.ToListAsync());
    }

    [Fact]
    public async Task Import_ReplacesSnapshot_WhenUniqueValuesMoveToNewIds()
    {
        await ImportAsync(Document("Original"));
        var replacementActorId = Guid.NewGuid();
        var replacementAlbumId = Guid.NewGuid();
        var replacementPostId = Guid.NewGuid();
        var replacement = Document("Replacement")
            .Replace(ActorId, replacementActorId.ToString(), StringComparison.Ordinal)
            .Replace(AlbumId, replacementAlbumId.ToString(), StringComparison.Ordinal)
            .Replace(PostId, replacementPostId.ToString(), StringComparison.Ordinal);

        await ImportAsync(replacement);

        await using var scope = fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        Assert.Equal(replacementActorId, (await context.Actors.SingleAsync()).Id);
        Assert.Equal(replacementAlbumId, (await context.Albums.SingleAsync()).Id);
        Assert.Equal(replacementPostId, (await context.Posts.SingleAsync()).Id);
    }

    [Fact]
    public async Task Import_RejectsMissingReferencesBeforeChangingDatabase()
    {
        await ImportAsync(Document("Original"));
        var unknownActorId = Guid.NewGuid();
        var invalid = Document("Invalid").Replace(
            $"\"actorId\": \"{ActorId}\"",
            $"\"actorId\": \"{unknownActorId}\"",
            StringComparison.Ordinal);

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => ImportAsync(invalid));

        Assert.Contains("absent from the import snapshot", exception.Message);
        await using var scope = fixture.Services.CreateAsyncScope();
        var actor = await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Actors.SingleAsync();
        Assert.Equal(Guid.Parse(ActorId), actor.Id);
    }

    [Fact]
    public async Task Import_RejectsMissingAlbumReferenceBeforeChangingDatabase()
    {
        await ImportAsync(Document("Original"));
        var unknownAlbumId = Guid.NewGuid();
        var invalid = Document("Invalid").Replace(
            $"\"albumId\": \"{AlbumId}\"",
            $"\"albumId\": \"{unknownAlbumId}\"",
            StringComparison.Ordinal);

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => ImportAsync(invalid));

        Assert.Contains("absent from the import snapshot", exception.Message);
        await using var scope = fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        Assert.Equal(Guid.Parse(PostId), (await context.Posts.SingleAsync()).Id);
    }

    [Fact]
    public async Task Import_RejectsUndefinedMediaType()
    {
        var invalid = Document("Invalid").Replace("\"Photo\"", "\"Audio\"", StringComparison.Ordinal);
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => ImportAsync(invalid));
        Assert.Contains("mediaType", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(", \"isSeries\": false", "isSeries")]
    [InlineData(", \"width\": 1200", "width")]
    [InlineData(", \"mediaType\": \"Photo\"", "mediaType")]
    public async Task Import_RejectsOmittedScalarConstructorParameters(string jsonProperty, string field)
    {
        var invalid = Document("Missing scalar").Replace(jsonProperty, string.Empty, StringComparison.Ordinal);

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => ImportAsync(invalid));

        Assert.Contains(field, exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Discovery_TagFilterIncludesTagsFoundOnlyOnPosts()
    {
        var document = Document("Post tag").Replace(
            "\"tags\": [\"history\"], \"editorialRank\": 10",
            "\"tags\": [\"post-only\"], \"editorialRank\": 10",
            StringComparison.Ordinal);
        await ImportAsync(document);

        await using var scope = fixture.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ICatalogReader>()
            .DiscoverAsync(null, "post-only", 1, 10, default);

        Assert.Equal(Guid.Parse(AlbumId), Assert.Single(result.Items).Id);
    }

    [Fact]
    public async Task PagedReaders_UseRepeatableReadTransactions()
    {
        await ImportAsync(PagingDocument());
        await using var scope = fixture.Services.CreateAsyncScope();
        var registeredContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var interceptor = new TransactionIsolationInterceptor();
        var options = new DbContextOptionsBuilder<CatalogDbContext>();
        CatalogModule.Configure(options, registeredContext.Database.GetConnectionString()!);
        options.AddInterceptors(interceptor);
        await using var context = new CatalogDbContext(options.Options);
        var reader = new CatalogReader(context);

        _ = await reader.GetActorsAsync(1, 10, default);
        _ = await reader.GetActorAlbumsAsync("ada-lovelace", 1, 10, default);
        _ = await reader.GetAlbumPostsAsync("ada-lovelace", "album-1", 1, 10, default);
        _ = await reader.DiscoverAsync(null, null, 1, 10, default);

        Assert.NotEmpty(interceptor.IsolationLevels);
        Assert.All(interceptor.IsolationLevels, level => Assert.Equal(IsolationLevel.RepeatableRead, level));
    }

    [Fact]
    public async Task Paging_IsStableBoundedAndReturnsEmptyOutOfRangePages()
    {
        await ImportAsync(PagingDocument());

        await using var scope = fixture.Services.CreateAsyncScope();
        var reader = scope.ServiceProvider.GetRequiredService<ICatalogReader>();
        var firstAlbums = await reader.GetActorAlbumsAsync("ada-lovelace", 1, 2, default);
        var secondAlbums = await reader.GetActorAlbumsAsync("ada-lovelace", 2, 2, default);
        var firstPostPage = await reader.GetAlbumPostsAsync("ada-lovelace", "album-1", 1, 1, default);
        var secondPostPage = await reader.GetAlbumPostsAsync("ada-lovelace", "album-1", 2, 1, default);
        var beyond = await reader.DiscoverAsync(null, null, 100, 10, default);
        var invalid = await scope.ServiceProvider.GetRequiredService<ISender>().Send(
            new DiscoverCatalogQuery(null, null, CatalogPagination.MaximumPage + 1, 1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            reader.DiscoverAsync(null, null, int.MaxValue, CatalogPagination.MaximumPageSize, default));

        Assert.NotNull(firstAlbums);
        Assert.Equal([Guid.Parse("22222222-2222-2222-2222-222222222221"), Guid.Parse("22222222-2222-2222-2222-222222222222")],
            firstAlbums.Items.Select(item => item.Id));
        Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222223"), Assert.Single(secondAlbums!.Items).Id);
        Assert.Equal(0, Assert.Single(firstPostPage!.Items).DisplayOrder);
        Assert.True(firstPostPage.HasNext);
        Assert.Equal(1, Assert.Single(secondPostPage!.Items).DisplayOrder);
        Assert.True(secondPostPage.HasPrev);
        Assert.Empty(beyond.Items);
        Assert.True(beyond.HasPrev);
        Assert.False(beyond.HasNext);
        Assert.True(invalid.IsFailure);
        Assert.Equal("Catalog.InvalidPage", invalid.Error.Code);
    }

    [Fact]
    public async Task ActorPaging_IsStableAndBounded()
    {
        await ImportAsync(ActorPagingDocument());

        await using var scope = fixture.Services.CreateAsyncScope();
        var reader = scope.ServiceProvider.GetRequiredService<ICatalogReader>();
        var first = await reader.GetActorsAsync(1, 2, default);
        var second = await reader.GetActorsAsync(2, 2, default);
        var invalid = await scope.ServiceProvider.GetRequiredService<ISender>().Send(
            new GetActorsQuery(1, CatalogPagination.MaximumPageSize + 1));
        var query = await scope.ServiceProvider.GetRequiredService<ISender>().Send(new GetActorsQuery(1, 2));

        Assert.Equal(
            [Guid.Parse("11111111-1111-1111-1111-111111111111"), Guid.Parse("11111111-1111-1111-1111-111111111112")],
            first.Items.Select(actor => actor.Id));
        Assert.Equal(
            [Guid.Parse("11111111-1111-1111-1111-111111111113"), Guid.Parse("11111111-1111-1111-1111-111111111114")],
            second.Items.Select(actor => actor.Id));
        Assert.True(first.HasNext);
        Assert.True(second.HasPrev);
        Assert.True(invalid.IsFailure);
        Assert.Equal("Catalog.InvalidPage", invalid.Error.Code);
        Assert.Equal(first.Items.Select(actor => actor.Id), query.Value.Items.Select(actor => actor.Id));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            reader.GetActorsAsync(CatalogPagination.MaximumPage + 1, 1, default));
    }

    [Fact]
    public async Task Post_CanBeLookedUpByStableId()
    {
        await ImportAsync(Document("Post lookup"));

        await using var scope = fixture.Services.CreateAsyncScope();
        var post = await scope.ServiceProvider.GetRequiredService<ICatalogReader>()
            .GetPostByIdAsync(Guid.Parse(PostId), default);

        Assert.NotNull(post);
        Assert.Equal("engine-diagram", post.Slug);
        Assert.Equal("photo", post.MediaType);
    }

    [Fact]
    public async Task PostByIdQuery_ReturnsImportedPost()
    {
        await ImportAsync(Document("Post query"));

        await using var scope = fixture.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ISender>()
            .Send(new GetPostByIdQuery(Guid.Parse(PostId)));

        Assert.True(result.IsSuccess);
        Assert.Equal("engine-diagram", result.Value.Slug);
    }

    [Fact]
    public async Task LegacyRoute_ResolvesNormalizedSourcePath()
    {
        await ImportAsync(Document("Legacy route"));

        await using var scope = fixture.Services.CreateAsyncScope();
        var route = await scope.ServiceProvider.GetRequiredService<ICatalogReader>()
            .ResolveLegacyRouteAsync("/OLD/ADA/", default);

        Assert.NotNull(route);
        Assert.Equal(Guid.Parse(LegacyRouteId), route.Id);
        Assert.Equal("/old/ada", route.SourcePath);
        Assert.Equal("/actors/ada-lovelace", route.DestinationPath);
    }

    [Fact]
    public async Task LegacyRouteQuery_ResolvesNormalizedSourcePath()
    {
        await ImportAsync(Document("Legacy query"));

        await using var scope = fixture.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ISender>()
            .Send(new ResolveLegacyRouteQuery("/OLD/ADA/"));

        Assert.True(result.IsSuccess);
        Assert.Equal("/actors/ada-lovelace", result.Value.DestinationPath);
    }

    [Theory]
    [InlineData("https://evil.example/path")]
    [InlineData("//evil.example/path")]
    [InlineData("/%2f%2fevil.example")]
    [InlineData("/safe/../evil")]
    public async Task Import_RejectsExternalOrUnsafeLegacyDestinations(string destination)
    {
        var invalid = Document("Invalid route").Replace(
            "/actors/ada-lovelace", destination, StringComparison.Ordinal);

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => ImportAsync(invalid));

        Assert.Contains("destinationPath", exception.Message);
    }

    [Fact]
    public async Task Import_RejectsNormalizedLegacySelfRedirect()
    {
        var invalid = Document("Self redirect").Replace(
            "/actors/ada-lovelace", "/OLD/ADA", StringComparison.Ordinal);

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => ImportAsync(invalid));

        Assert.Contains("itself", exception.Message);
    }

    [Fact]
    public async Task Import_RejectsNormalizedLegacyRedirectCycles()
    {
        var invalid = LegacyRoutesDocument("""
            {"id":"44444444-4444-4444-4444-444444444441","sourcePath":"/A","destinationPath":"/B","createdAt":"2026-01-01T00:00:00Z","updatedAt":"2026-01-01T00:00:00Z"},
            {"id":"44444444-4444-4444-4444-444444444442","sourcePath":"/b","destinationPath":"/a","createdAt":"2026-01-01T00:00:00Z","updatedAt":"2026-01-01T00:00:00Z"}
            """);

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => ImportAsync(invalid));

        Assert.Contains("cycle", exception.Message);
    }

    [Theory]
    [InlineData("/images/file.jpg")]
    [InlineData("images/%2e%2e/secret.jpg")]
    [InlineData("images/file.jpg?download=1")]
    [InlineData("images/file.jpg#preview")]
    [InlineData("https://example.com/file.jpg")]
    [InlineData("images\\file.jpg")]
    [InlineData("images/./file.jpg")]
    [InlineData("images/../file.jpg")]
    [InlineData("images/\u0001file.jpg")]
    public void DomainEntities_RejectUnsafeStorageKeys(string storageKey)
    {
        var published = DateTimeOffset.Parse("2026-01-01T00:00:00Z");

        Assert.Throws<ArgumentException>(() => new Actor(
            Guid.NewGuid(), "actor", "Name", "Profession", "Bio", storageKey,
            0, 0, published, published));
    }

    [Fact]
    public async Task DatabaseConstraints_RejectUnsafeStorageKeysForAllEntities()
    {
        await ImportAsync(Document("Storage constraints"));
        await using var scope = fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        await AssertStorageConstraintAsync(context,
            "UPDATE \"Catalog\".\"Actors\" SET \"AvatarStorageKey\" = {0} WHERE \"Id\" = {1}",
            Guid.Parse(ActorId), "CK_Actors_AvatarStorageKey");
        await AssertStorageConstraintAsync(context,
            "UPDATE \"Catalog\".\"Albums\" SET \"CoverStorageKey\" = {0} WHERE \"Id\" = {1}",
            Guid.Parse(AlbumId), "CK_Albums_CoverStorageKey");
        await AssertStorageConstraintAsync(context,
            "UPDATE \"Catalog\".\"Posts\" SET \"StorageKey\" = {0} WHERE \"Id\" = {1}",
            Guid.Parse(PostId), "CK_Posts_StorageKey");
    }

    [Theory]
    [InlineData("/images/file.jpg")]
    [InlineData("images/%2e%2e/secret.jpg")]
    [InlineData("images/file.jpg?download=1")]
    [InlineData("images/file.jpg#preview")]
    [InlineData("https://example.com/file.jpg")]
    [InlineData("images\\file.jpg")]
    [InlineData("images/./file.jpg")]
    [InlineData("images/../file.jpg")]
    [InlineData("images/\u0001file.jpg")]
    public async Task DatabaseConstraint_RejectsEveryUnsafeStorageKeyForm(string storageKey)
    {
        await ImportAsync(Document("Storage constraint forms"));
        await using var scope = fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            context.Database.ExecuteSqlRawAsync(
                "UPDATE \"Catalog\".\"Actors\" SET \"AvatarStorageKey\" = {0} WHERE \"Id\" = {1}",
                storageKey, Guid.Parse(ActorId)));

        Assert.Equal("CK_Actors_AvatarStorageKey", exception.ConstraintName);
    }

    [Fact]
    public void DomainEntities_RejectInvalidInvariants()
    {
        var published = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var updated = published.AddDays(1);

        Assert.Throws<ArgumentException>(() => new Actor(
            Guid.Empty, "bad--slug", "Name", "Profession", "Bio", "avatar.jpg", 0, 0, published, updated));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Album(
            Guid.NewGuid(), Guid.NewGuid(), "album", "Title", "Description", "cover.jpg", "Cover", double.NaN,
            false, [], 0, published, updated));
        Assert.Throws<ArgumentOutOfRangeException>(() => ValidPost((MediaType)999, null));
        Assert.Throws<ArgumentException>(() => ValidPost(MediaType.Photo, 1));
        Assert.Throws<ArgumentException>(() => ValidPost(MediaType.Video, 0));
        Assert.Throws<ArgumentException>(() => new Actor(
            Guid.NewGuid(), "actor", "Name", "Profession", "Bio", "../secret", 0, 0, published, updated));
        Assert.Throws<ArgumentException>(() => new Album(
            Guid.NewGuid(), Guid.NewGuid(), "album", "Title", "Description", "cover.jpg", "Cover", 1,
             false, [new string('a', 101)], 0, published, updated));
        Assert.Throws<ArgumentException>(() => new LegacyRoute(
            Guid.NewGuid(), "/old", "https://evil.example", published, updated));
        Assert.Equal(
            "/",
            new LegacyRoute(Guid.NewGuid(), "/old-home", "/", published, updated)
                .DestinationPath);
    }

    private async Task ImportAsync(string json, bool dryRun = false)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        await scope.ServiceProvider.GetRequiredService<CatalogJsonImporter>()
            .ImportAsync(stream, dryRun);
    }

    private static async Task AssertStorageConstraintAsync(
        CatalogDbContext context,
        string sql,
        Guid id,
        string constraintName)
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            context.Database.ExecuteSqlRawAsync(sql, "images/%2e%2e/secret.jpg", id));
        Assert.Equal(constraintName, exception.ConstraintName);
    }

    private static string Document(string title) => $$"""
        {
          "actors": [{
            "id": "{{ActorId}}", "slug": "ada-lovelace", "displayName": "Ada Lovelace",
            "profession": "Mathematician", "bio": "Pioneer", "avatarStorageKey": "actors/ada.jpg",
            "followerCount": 10, "editorialRank": 100,
            "publishedAt": "2026-01-01T00:00:00Z", "updatedAt": "2026-01-02T00:00:00Z"
          }],
          "albums": [{
            "id": "{{AlbumId}}", "actorId": "{{ActorId}}", "slug": "analytical-engine",
            "title": "{{title}}", "description": "Engine notes", "coverStorageKey": "albums/engine.jpg",
            "coverAltText": "Engine diagram", "coverAspectRatio": 1.5, "isSeries": false,
            "tags": ["History", "science"], "editorialRank": 50,
            "publishedAt": "2026-01-01T00:00:00Z", "updatedAt": "2026-01-03T00:00:00Z"
          }],
          "posts": [{
            "id": "{{PostId}}", "albumId": "{{AlbumId}}", "slug": "engine-diagram",
            "storageKey": "posts/engine.jpg", "mediaType": "Photo", "width": 1200, "height": 800,
            "durationSeconds": null, "mimeType": "image/jpeg", "byteSize": 12345,
            "caption": "A diagram", "altText": "Analytical engine diagram", "displayOrder": 0,
            "tags": ["history"], "editorialRank": 10,
            "publishedAt": "2026-01-01T00:00:00Z", "updatedAt": "2026-01-01T00:00:00Z"
          }],
          "legacyRoutes": [{
            "id": "{{LegacyRouteId}}", "sourcePath": "/Old/Ada/", "destinationPath": "/actors/ada-lovelace",
            "createdAt": "2026-01-01T00:00:00Z", "updatedAt": "2026-01-02T00:00:00Z"
          }]
        }
        """;

    private static Post ValidPost(MediaType mediaType, int? duration) => new(
        Guid.NewGuid(), Guid.NewGuid(), "valid-post", "post.jpg", mediaType, 100, 100,
        duration, "image/jpeg", 1, null, "Alt", 0, [], 0,
        DateTimeOffset.Parse("2026-01-01T00:00:00Z"), DateTimeOffset.Parse("2026-01-02T00:00:00Z"));

    private static string PagingDocument()
    {
        var albums = string.Join(',', Enumerable.Range(1, 3).Select(index => $$"""
            {
              "id": "22222222-2222-2222-2222-22222222222{{index}}", "actorId": "{{ActorId}}", "slug": "album-{{index}}",
              "title": "Album {{index}}", "description": "Description", "coverStorageKey": "albums/{{index}}.jpg",
              "coverAltText": "Cover", "coverAspectRatio": 1.5, "isSeries": false, "tags": [" Z ", "a", "A"],
              "editorialRank": 10, "publishedAt": "2026-01-01T00:00:00Z", "updatedAt": "2026-01-02T00:00:00Z"
            }
            """));
        var posts = string.Join(',', Enumerable.Range(0, 2).Select(index => $$"""
            {
              "id": "33333333-3333-3333-3333-33333333333{{index}}", "albumId": "22222222-2222-2222-2222-222222222221",
              "slug": "post-{{index}}", "storageKey": "posts/{{index}}.jpg", "mediaType": "Photo", "width": 100,
              "height": 100, "durationSeconds": null, "mimeType": "image/jpeg", "byteSize": 1,
              "caption": null, "altText": "Post", "displayOrder": {{index}}, "tags": [], "editorialRank": 0,
              "publishedAt": "2026-01-01T00:00:00Z", "updatedAt": "2026-01-01T00:00:00Z"
            }
            """));

        return $$"""
            {
              "actors": [{
                "id": "{{ActorId}}", "slug": "ada-lovelace", "displayName": "Ada Lovelace", "profession": "Mathematician",
                "bio": "Pioneer", "avatarStorageKey": "actors/ada.jpg", "followerCount": 10, "editorialRank": 10,
                "publishedAt": "2026-01-01T00:00:00Z", "updatedAt": "2026-01-02T00:00:00Z"
              }],
              "albums": [{{albums}}],
              "posts": [{{posts}}],
              "legacyRoutes": []
            }
            """;
    }

    private const string EmptyDocument = """{"actors":[],"albums":[],"posts":[],"legacyRoutes":[]}""";

    private static string LegacyRoutesDocument(string routes) =>
        $$"""{"actors":[],"albums":[],"posts":[],"legacyRoutes":[{{routes}}]}""";

    private static string ActorPagingDocument()
    {
        var actors = string.Join(',', new[]
        {
            ("11111111-1111-1111-1111-111111111111", "actor-one", 10, "2026-01-02T00:00:00Z"),
            ("11111111-1111-1111-1111-111111111112", "actor-two", 10, "2026-01-02T00:00:00Z"),
            ("11111111-1111-1111-1111-111111111113", "actor-three", 10, "2026-01-01T00:00:00Z"),
            ("11111111-1111-1111-1111-111111111114", "actor-four", 5, "2026-01-03T00:00:00Z")
        }.Select(actor => $$"""
            {
              "id": "{{actor.Item1}}", "slug": "{{actor.Item2}}", "displayName": "{{actor.Item2}}",
              "profession": "Profession", "bio": "Bio", "avatarStorageKey": "actors/{{actor.Item2}}.jpg",
              "followerCount": 0, "editorialRank": {{actor.Item3}},
              "publishedAt": "{{actor.Item4}}", "updatedAt": "{{actor.Item4}}"
            }
            """));

        return $$"""{"actors":[{{actors}}],"albums":[],"posts":[],"legacyRoutes":[]}""";
    }

    private sealed class TransactionIsolationInterceptor : DbCommandInterceptor
    {
        internal List<IsolationLevel?> IsolationLevels { get; } = [];

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            IsolationLevels.Add(command.Transaction?.IsolationLevel);
            return ValueTask.FromResult(result);
        }
    }
}
