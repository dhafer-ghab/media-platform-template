using Moq;
using Posts.Application.Abstractions;
using Posts.Application.Posts;
using Posts.Application.Posts.Queries.GetPosts;

namespace Posts.UnitTests;

public sealed class GetPostsQueryHandlerTests
{
    [Fact]
    public async Task Handle_RejectsUnknownSortValue()
    {
        var repository = new Mock<IPostRepository>();
        var handler = new GetPostsQueryHandler(repository.Object);

        var result = await handler.Handle(
            new GetPostsQuery(null, null, null, (PostSort)99), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Posts.SortInvalid", result.Error.Code);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_RejectsOversizedPage()
    {
        var repository = new Mock<IPostRepository>();
        var handler = new GetPostsQueryHandler(repository.Object);

        var result = await handler.Handle(
            new GetPostsQuery(null, null, null, PostSort.Newest, 1, 101), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Posts.PageSizeInvalid", result.Error.Code);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_CleansFiltersBeforeQueryingRepository()
    {
        var expected = new PagedPostsDto([], 1, 20, 0, 0);
        var repository = new Mock<IPostRepository>();
        repository
            .Setup(value => value.GetPublishedAsync(
                "studio", "Painting", "abstract", PostSort.Popular, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var handler = new GetPostsQueryHandler(repository.Object);

        var result = await handler.Handle(
            new GetPostsQuery(" studio ", " Painting ", " #abstract ", PostSort.Popular),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(expected, result.Value);
    }
}
