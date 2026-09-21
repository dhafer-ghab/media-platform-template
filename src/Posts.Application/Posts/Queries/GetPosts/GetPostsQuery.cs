using SharedKernal.Messaging;
using SharedKernal.Results;

namespace Posts.Application.Posts.Queries.GetPosts;

public sealed record GetPostsQuery(
    string? Q,
    string? Category,
    string? Tag,
    PostSort Sort = PostSort.Newest,
    int Page = 1,
    int PageSize = 20) : IQuery<Result<PagedPostsDto>>;
