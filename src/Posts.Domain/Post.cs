using SharedKernal.Entities;
using SharedKernal.Results;

namespace Posts.Domain;

public sealed class Post : AggregateRoot
{
    private readonly List<Tag> _tags = [];

    private Post(
        Guid id,
        Guid authorId,
        Guid mediaAssetId,
        string title,
        string description,
        string category,
        string mediaUrl,
        string altText) : base(id)
    {
        AuthorId = authorId;
        MediaAssetId = mediaAssetId;
        Title = title;
        Description = description;
        Category = category;
        NormalizedCategory = NormalizeCategory(category);
        MediaUrl = mediaUrl;
        AltText = altText;
        Status = PostStatus.Draft;
    }

    private Post()
    {
        Title = null!;
        Description = null!;
        Category = null!;
        NormalizedCategory = null!;
        MediaUrl = null!;
        AltText = null!;
    }

    public Guid AuthorId { get; private set; }
    public Guid MediaAssetId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public string NormalizedCategory { get; private set; }
    public string MediaUrl { get; private set; }
    public string AltText { get; private set; }
    public PostStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public long ViewCount { get; private set; }
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    public static Result<Post> Create(
        Guid authorId,
        Guid mediaAssetId,
        string? title,
        string? description,
        string? category,
        string? mediaUrl,
        string? altText)
    {
        if (authorId == Guid.Empty)
            return Error.Validation("Post.AuthorRequired", "An author is required.");
        if (mediaAssetId == Guid.Empty)
            return Error.Validation("Post.MediaRequired", "A media asset is required.");
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200)
            return Error.Validation("Post.TitleInvalid", "Title is required and cannot exceed 200 characters.");
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > 2_000)
            return Error.Validation("Post.DescriptionInvalid", "Description is required and cannot exceed 2000 characters.");
        if (string.IsNullOrWhiteSpace(category) || category.Trim().Length > 100)
            return Error.Validation("Post.CategoryInvalid", "Category is required and cannot exceed 100 characters.");
        if (string.IsNullOrWhiteSpace(mediaUrl) || mediaUrl.Trim().Length > 2_048)
            return Error.Validation("Post.MediaUrlInvalid", "Media URL is required and cannot exceed 2048 characters.");
        if (string.IsNullOrWhiteSpace(altText) || altText.Trim().Length > 500)
            return Error.Validation("Post.AltTextInvalid", "Alt text is required and cannot exceed 500 characters.");

        return new Post(
            Guid.NewGuid(), authorId, mediaAssetId, title.Trim(), description.Trim(), category.Trim(),
            mediaUrl.Trim(), altText.Trim());
    }

    public Result AddTag(Tag tag)
    {
        if (string.IsNullOrWhiteSpace(tag.Name) || tag.Name.Length > 50)
            return Result.Failure(Error.Validation("Post.TagInvalid", "Tags cannot be empty or exceed 50 characters."));

        if (_tags.All(existing => existing.NormalizedName != tag.NormalizedName))
            _tags.Add(tag);

        return Result.Success();
    }

    public Result Publish(DateTime utcNow)
    {
        if (Status == PostStatus.Published)
            return Result.Failure(Error.Conflict("Post.AlreadyPublished", "Post is already published."));

        Status = PostStatus.Published;
        PublishedAt = utcNow.Kind == DateTimeKind.Utc ? utcNow : utcNow.ToUniversalTime();
        UpdateDate = PublishedAt;
        return Result.Success();
    }

    private static string NormalizeCategory(string value) => value.Trim().ToLowerInvariant();
}
