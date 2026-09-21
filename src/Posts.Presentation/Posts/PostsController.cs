using MediatR;
using Microsoft.AspNetCore.Mvc;
using Posts.Application.Posts;
using Posts.Application.Posts.Commands.RecordPostView;
using Posts.Application.Posts.Queries.GetPostById;
using Posts.Application.Posts.Queries.GetPostFacets;
using Posts.Application.Posts.Queries.GetPosts;
using SharedKernal.Extensions;

namespace Posts.Presentation.Posts;

[ApiController]
[Route("api/posts")]
public sealed class PostsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? q,
        [FromQuery] string? category,
        [FromQuery] string? tag,
        [FromQuery] PostSort sort = PostSort.Newest,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetPostsQuery(q, category, tag, sort, page, pageSize), cancellationToken);
        return result.Match(posts => Ok(posts));
    }

    [HttpGet("facets")]
    public async Task<IActionResult> GetFacets(CancellationToken cancellationToken)
    {
        var facets = await sender.Send(new GetPostFacetsQuery(), cancellationToken);
        return Ok(facets);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPostByIdQuery(id), cancellationToken);
        return result.Match(post => Ok(post));
    }

    [HttpPost("{id:guid}/views")]
    public async Task<IActionResult> RecordView(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RecordPostViewCommand(id), cancellationToken);
        return result.Match(NoContent());
    }
}
