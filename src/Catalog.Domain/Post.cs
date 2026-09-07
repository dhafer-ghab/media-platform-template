namespace Catalog.Domain;

public enum MediaType
{
    Photo,
    Video
}

public sealed class Post
{
    private Post() { }

    public Post(
        Guid id,
        Guid albumId,
        string slug,
        string storageKey,
        MediaType mediaType,
        int width,
        int height,
        int? durationSeconds,
        string mimeType,
        long byteSize,
        string? caption,
        string altText,
        int displayOrder,
        string[] tags,
        int editorialRank,
        DateTimeOffset publishedAt,
        DateTimeOffset updatedAt)
    {
        CatalogValidation.Id(id, nameof(id));
        Id = id;
        Update(albumId, slug, storageKey, mediaType, width, height, durationSeconds,
            mimeType, byteSize, caption, altText, displayOrder, tags, editorialRank,
            publishedAt, updatedAt);
    }

    public Guid Id { get; private set; }
    public Guid AlbumId { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public MediaType MediaType { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int? DurationSeconds { get; private set; }
    public string MimeType { get; private set; } = string.Empty;
    public long ByteSize { get; private set; }
    public string? Caption { get; private set; }
    public string AltText { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public string[] Tags { get; private set; } = [];
    public int EditorialRank { get; private set; }
    public DateTimeOffset PublishedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Album Album { get; private set; } = null!;

    public void Update(
        Guid albumId,
        string slug,
        string storageKey,
        MediaType mediaType,
        int width,
        int height,
        int? durationSeconds,
        string mimeType,
        long byteSize,
        string? caption,
        string altText,
        int displayOrder,
        string[] tags,
        int editorialRank,
        DateTimeOffset publishedAt,
        DateTimeOffset updatedAt)
    {
        CatalogValidation.Id(albumId, nameof(albumId));
        if (!Enum.IsDefined(mediaType))
            throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, "mediaType must be Photo or Video.");
        CatalogValidation.Positive(width, nameof(width));
        CatalogValidation.Positive(height, nameof(height));
        if (byteSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(byteSize), byteSize, "byteSize must be positive.");
        CatalogValidation.NonNegative(displayOrder, nameof(displayOrder));
        CatalogValidation.NonNegative(editorialRank, nameof(editorialRank));
        if (mediaType == MediaType.Photo && durationSeconds is not null)
            throw new ArgumentException("Photo durationSeconds must be null.", nameof(durationSeconds));
        if (mediaType == MediaType.Video && durationSeconds is null or <= 0)
            throw new ArgumentException("Video durationSeconds must be positive.", nameof(durationSeconds));
        var validatedMimeType = CatalogValidation.Required(mimeType, 100, nameof(mimeType));
        if (mediaType == MediaType.Photo && !validatedMimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Photo mimeType must be an image media type.", nameof(mimeType));
        if (mediaType == MediaType.Video && !validatedMimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Video mimeType must be a video media type.", nameof(mimeType));
        var timestamps = CatalogValidation.Timestamps(publishedAt, updatedAt);

        AlbumId = albumId;
        Slug = CatalogValidation.Slug(slug, nameof(slug));
        StorageKey = CatalogValidation.StorageKey(storageKey, nameof(storageKey));
        MediaType = mediaType;
        Width = width;
        Height = height;
        DurationSeconds = durationSeconds;
        MimeType = validatedMimeType;
        ByteSize = byteSize;
        Caption = CatalogValidation.Optional(caption, 2000, nameof(caption));
        AltText = CatalogValidation.Required(altText, 500, nameof(altText));
        DisplayOrder = displayOrder;
        Tags = CatalogValidation.Tags(tags);
        EditorialRank = editorialRank;
        PublishedAt = timestamps.PublishedAt;
        UpdatedAt = timestamps.UpdatedAt;
    }
}
