using Host.WebApi.ArtworkViews;
using Microsoft.EntityFrameworkCore;

namespace Users.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class ArtworkViewStoreTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task IncrementAsync_ConcurrentlyIncrementsOneCounter()
    {
        const string slug = "market-at-noon-0";
        await DeleteAsync(slug);

        await Task.WhenAll(Enumerable.Range(0, 10).Select(async _ =>
        {
            await using var context = new ArtworkViewsDbContext(fixture.ArtworkViewsOptions);
            await new ArtworkViewStore(context).IncrementAsync(slug, CancellationToken.None);
        }));

        await using var verificationContext = new ArtworkViewsDbContext(fixture.ArtworkViewsOptions);
        var count = await verificationContext.ArtworkViews
            .Where(view => view.ArtworkSlug == slug)
            .Select(view => view.ViewCount)
            .SingleAsync();

        Assert.Equal(10, count);
    }

    [Fact]
    public async Task GetTopAsync_ExcludesUnknownSlugsAndRanksKnownSlugs()
    {
        await DeleteAsync("market-at-noon-0", "rainband-12", "unknown-artwork");
        await IncrementAsync("market-at-noon-0", 2);
        await IncrementAsync("rainband-12", 3);
        await IncrementAsync("unknown-artwork", 4);

        await using var context = new ArtworkViewsDbContext(fixture.ArtworkViewsOptions);
        var items = await new ArtworkViewStore(context).GetTopAsync(
            2,
            ["market-at-noon-0", "rainband-12"],
            CancellationToken.None);

        Assert.Collection(
            items,
            item => Assert.Equal(("rainband-12", 3L), (item.Slug, item.ViewCount)),
            item => Assert.Equal(("market-at-noon-0", 2L), (item.Slug, item.ViewCount)));
    }

    [Fact]
    public async Task Migration_CreatesAnalyticsTable()
    {
        await using var context = new ArtworkViewsDbContext(fixture.ArtworkViewsOptions);
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT to_regclass('analytics.artwork_view_counts') IS NOT NULL";
        await context.Database.OpenConnectionAsync();

        Assert.True(Convert.ToBoolean(await command.ExecuteScalarAsync()));
    }

    private async Task IncrementAsync(string slug, int count)
    {
        for (var index = 0; index < count; index++)
        {
            await using var context = new ArtworkViewsDbContext(fixture.ArtworkViewsOptions);
            await new ArtworkViewStore(context).IncrementAsync(slug, CancellationToken.None);
        }
    }

    private async Task DeleteAsync(params string[] slugs)
    {
        await using var context = new ArtworkViewsDbContext(fixture.ArtworkViewsOptions);
        foreach (var slug in slugs)
            await context.ArtworkViews.Where(view => view.ArtworkSlug == slug).ExecuteDeleteAsync();
    }
}
