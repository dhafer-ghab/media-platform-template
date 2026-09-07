using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Host.WebApi.ArtworkViews;

internal static class ArtworkViewEndpoints
{
    public static IEndpointRouteBuilder MapArtworkViewEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/artwork-views")
            .AllowAnonymous()
            .RequireCors("public-frontend")
            .WithTags("Artwork Views");

        group.MapPost("/", async (
                HttpRequest httpRequest,
                ArtworkSlugCatalog catalog,
                ArtworkViewStore store,
                CancellationToken cancellationToken) =>
            {
                if (!httpRequest.HasJsonContentType())
                {
                    return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
                }
                if (httpRequest.ContentLength is > 1024)
                {
                    return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
                }

                await using var body = new MemoryStream();
                var buffer = new byte[512];
                while (true)
                {
                    var read = await httpRequest.Body.ReadAsync(buffer, cancellationToken);
                    if (read == 0) break;
                    if (body.Length + read > 1024)
                    {
                        return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
                    }
                    await body.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }
                body.Position = 0;

                RecordArtworkViewRequest? request;
                try
                {
                    request = await JsonSerializer.DeserializeAsync<RecordArtworkViewRequest>(
                        body,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web),
                        cancellationToken: cancellationToken);
                }
                catch (JsonException)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["body"] = ["A valid JSON request body is required."]
                    });
                }

                var slug = request?.Slug?.Trim();
                if (string.IsNullOrEmpty(slug) || slug.Length > 120 || !ArtworkSlugAttribute.IsValidSlug(slug))
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["slug"] = ["A valid artwork slug is required."]
                    });
                }
                if (!catalog.Contains(slug))
                {
                    return Results.NotFound();
                }

                await store.IncrementAsync(slug, cancellationToken);
                return Results.NoContent();
            })
            .RequireRateLimiting("artwork-view-recording");

        group.MapGet("/top", async (
                int? limit,
                ArtworkSlugCatalog catalog,
                ArtworkViewStore store,
                CancellationToken cancellationToken) =>
            {
                var requestedLimit = limit ?? 12;
                if (requestedLimit is < 1 or > 50)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["limit"] = ["Limit must be between 1 and 50."]
                    });
                }

                var items = await store.GetTopAsync(requestedLimit, catalog.Slugs, cancellationToken);
                return Results.Ok(new TopArtworkViewsResponse(items));
            })
            .RequireRateLimiting("artwork-view-ranking");

        return endpoints;
    }

    internal sealed record RecordArtworkViewRequest([property: ArtworkSlug] string? Slug);

    internal sealed record TopArtworkViewsResponse(IReadOnlyList<TopArtworkViewItem> Items);

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    private sealed class ArtworkSlugAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value) => value is string slug && IsValidSlug(slug);

        public static bool IsValidSlug(string slug) =>
            slug.Length is >= 1 and <= 120 &&
            slug.All(character => char.IsAsciiLetterOrDigit(character) || character == '-') &&
            slug == slug.ToLowerInvariant() &&
            !slug.StartsWith('-') &&
            !slug.EndsWith('-') &&
            !slug.Contains("--", StringComparison.Ordinal);
    }
}
