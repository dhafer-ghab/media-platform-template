namespace Host.WebApi.ArtworkViews;

internal sealed class ArtworkView
{
    public string ArtworkSlug { get; private set; } = string.Empty;

    public long ViewCount { get; private set; }

    public DateTimeOffset LastViewedAt { get; private set; }
}
