using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Posts.Application.Posts;
using Posts.Application.Posts.Commands.RecordPostView;
using Posts.Application.Posts.Queries.GetPosts;
using Posts.Presentation.Posts;
using SharedKernal.Results;

namespace Posts.UnitTests;

public sealed class PostsControllerTests
{
    [Fact]
    public async Task GetAll_ForwardsDiscoveryParameters()
    {
        var sender = new Mock<ISender>();
        sender.Setup(value => value.Send(It.IsAny<GetPostsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PagedPostsDto([], 2, 10, 0, 0)));
        var controller = new PostsController(sender.Object);

        var response = await controller.GetAll(
            "market", "Painting", "abstract", PostSort.Popular, 2, 10, CancellationToken.None);

        Assert.IsType<OkObjectResult>(response);
        sender.Verify(value => value.Send(
            new GetPostsQuery("market", "Painting", "abstract", PostSort.Popular, 2, 10),
            CancellationToken.None));
    }

    [Fact]
    public async Task RecordView_ReturnsNotFoundWhenPostIsNotPublished()
    {
        var id = Guid.NewGuid();
        var sender = new Mock<ISender>();
        sender.Setup(value => value.Send(new RecordPostViewCommand(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.NotFound("Post.NotFound", "Not found.")));
        var controller = new PostsController(sender.Object);

        var response = await controller.RecordView(id, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(response);
    }
}
