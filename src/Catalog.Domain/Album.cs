namespace Catalog.Domain;

public sealed class Album
{
    private Album() { }

    public Album(
        Guid id,
        Guid actorId,
        string slug,
        string title,
        string description,
        string coverStorageKey,
        string coverAltText,
        double coverAspectRatio,
        bool isSeries,
        string[] tags,
        int editorialRank,
        DateTimeOffset publishedAt,
        DateTimeOffset updatedAt)
    {
        CatalogValidation.Id(id, nameof(id));
        Id = id;
        Update(actorId, slug, title, description, coverStorageKey, coverAltText,
            coverAspectRatio, isSeries, tags, editorialRank, publishedAt, updatedAt);
    }

    public Guid Id { get; private set; }
    public Guid ActorId { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string CoverStorageKey { get; private set; } = string.Empty;
    public string CoverAltText { get; private set; } = string.Empty;
    public double CoverAspectRatio { get; private set; }
    public bool IsSeries { get; private set; }
    public string[] Tags { get; private set; } = [];
    public int EditorialRank { get; private set; }
    public DateTimeOffset PublishedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Actor Actor { get; private set; } = null!;

    public void Update(
        Guid actorId,
        string slug,
        string title,
        string description,
        string coverStorageKey,
        string coverAltText,
        double coverAspectRatio,
        bool isSeries,
        string[] tags,
        int editorialRank,
        DateTimeOffset publishedAt,
        DateTimeOffset updatedAt)
    {
        CatalogValidation.Id(actorId, nameof(actorId));
        CatalogValidation.Positive(coverAspectRatio, nameof(coverAspectRatio));
        CatalogValidation.NonNegative(editorialRank, nameof(editorialRank));
        var timestamps = CatalogValidation.Timestamps(publishedAt, updatedAt);

        ActorId = actorId;
        Slug = CatalogValidation.Slug(slug, nameof(slug));
        Title = CatalogValidation.Required(title, 250, nameof(title));
        Description = CatalogValidation.Required(description, 4000, nameof(description));
        CoverStorageKey = CatalogValidation.StorageKey(coverStorageKey, nameof(coverStorageKey));
        CoverAltText = CatalogValidation.Required(coverAltText, 500, nameof(coverAltText));
        CoverAspectRatio = coverAspectRatio;
        IsSeries = isSeries;
        Tags = CatalogValidation.Tags(tags);
        EditorialRank = editorialRank;
        PublishedAt = timestamps.PublishedAt;
        UpdatedAt = timestamps.UpdatedAt;
    }
}
