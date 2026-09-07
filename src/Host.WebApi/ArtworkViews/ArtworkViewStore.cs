using Microsoft.EntityFrameworkCore;

namespace Host.WebApi.ArtworkViews;

internal sealed class ArtworkViewStore(ArtworkViewsDbContext dbContext)
{
    public Task IncrementAsync(string slug, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO analytics.artwork_view_counts AS views
                (artwork_slug, view_count, last_viewed_at)
            VALUES
                ({slug}, 1, {DateTimeOffset.UtcNow})
            ON CONFLICT (artwork_slug) DO UPDATE
            SET view_count = views.view_count + 1,
                last_viewed_at = EXCLUDED.last_viewed_at;
            """, cancellationToken);

    public async Task<IReadOnlyList<TopArtworkViewItem>> GetTopAsync(
        int limit,
        IReadOnlyCollection<string> allowedSlugs,
        CancellationToken cancellationToken)
    {
        var allowed = allowedSlugs.ToList();
        return await dbContext.ArtworkViews
            .AsNoTracking()
            .Where(view => allowed.Contains(view.ArtworkSlug))
            .OrderByDescending(view => view.ViewCount)
            .ThenBy(view => view.ArtworkSlug)
            .Take(limit)
            .Select(view => new TopArtworkViewItem(view.ArtworkSlug, view.ViewCount))
            .ToListAsync(cancellationToken);
    }
}

internal sealed record TopArtworkViewItem(string Slug, long ViewCount);
