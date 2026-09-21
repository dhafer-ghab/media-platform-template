using Posts.Domain;

namespace Posts.UnitTests;

public sealed class PostTests
{
    [Fact]
    public void Create_RejectsMissingRequiredMetadata()
    {
        var result = Post.Create(Guid.Empty, Guid.NewGuid(), "Title", "Description", "Art", "https://media/post", "Alt");

        Assert.True(result.IsFailure);
        Assert.Equal("Post.AuthorRequired", result.Error.Code);
    }

    [Fact]
    public void AddTag_NormalizesAndDeduplicatesTags()
    {
        var post = CreatePost();

        post.AddTag(Tag.Create(" Abstract ").Value);
        post.AddTag(Tag.Create("#abstract").Value);

        var tag = Assert.Single(post.Tags);
        Assert.Equal("Abstract", tag.Name);
        Assert.Equal("abstract", tag.NormalizedName);
    }

    [Fact]
    public void Publish_SetsPublishedStateAndRejectsSecondPublish()
    {
        var post = CreatePost();
        var publishedAt = new DateTime(2026, 9, 16, 12, 0, 0, DateTimeKind.Utc);

        var first = post.Publish(publishedAt);
        var second = post.Publish(publishedAt.AddMinutes(1));

        Assert.True(first.IsSuccess);
        Assert.Equal(PostStatus.Published, post.Status);
        Assert.Equal(publishedAt, post.PublishedAt);
        Assert.True(second.IsFailure);
        Assert.Equal("Post.AlreadyPublished", second.Error.Code);
    }

    private static Post CreatePost() => Post.Create(
        Guid.NewGuid(), Guid.NewGuid(), "A title", "A description", "Painting",
        "https://media.example/post.jpg", "An accessible description").Value;
}
