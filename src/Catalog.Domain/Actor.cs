namespace Catalog.Domain;

public sealed class Actor
{
    private Actor() { }

    public Actor(
        Guid id,
        string slug,
        string displayName,
        string profession,
        string bio,
        string avatarStorageKey,
        long followerCount,
        int editorialRank,
        DateTimeOffset publishedAt,
        DateTimeOffset updatedAt)
    {
        CatalogValidation.Id(id, nameof(id));
        Id = id;
        Update(slug, displayName, profession, bio, avatarStorageKey, followerCount,
            editorialRank, publishedAt, updatedAt);
    }

    public Guid Id { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string Profession { get; private set; } = string.Empty;
    public string Bio { get; private set; } = string.Empty;
    public string AvatarStorageKey { get; private set; } = string.Empty;
    public long FollowerCount { get; private set; }
    public int EditorialRank { get; private set; }
    public DateTimeOffset PublishedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        string slug,
        string displayName,
        string profession,
        string bio,
        string avatarStorageKey,
        long followerCount,
        int editorialRank,
        DateTimeOffset publishedAt,
        DateTimeOffset updatedAt)
    {
        CatalogValidation.NonNegative(followerCount, nameof(followerCount));
        CatalogValidation.NonNegative(editorialRank, nameof(editorialRank));
        var timestamps = CatalogValidation.Timestamps(publishedAt, updatedAt);

        Slug = CatalogValidation.Slug(slug, nameof(slug));
        DisplayName = CatalogValidation.Required(displayName, 200, nameof(displayName));
        Profession = CatalogValidation.Required(profession, 200, nameof(profession));
        Bio = CatalogValidation.Required(bio, 4000, nameof(bio));
        AvatarStorageKey = CatalogValidation.StorageKey(avatarStorageKey, nameof(avatarStorageKey));
        FollowerCount = followerCount;
        EditorialRank = editorialRank;
        PublishedAt = timestamps.PublishedAt;
        UpdatedAt = timestamps.UpdatedAt;
    }
}
