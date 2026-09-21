using Posts.Application.Abstractions;
using SharedKernal.Messaging;
using SharedKernal.Results;

namespace Posts.Application.Posts.Queries.GetPosts;

internal sealed class GetPostsQueryHandler(IPostRepository repository)
    : IQueryHandler<GetPostsQuery, Result<PagedPostsDto>>
{
    public async Task<Result<PagedPostsDto>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        if (request.Page < 1)
            return Error.Validation("Posts.PageInvalid", "Page must be greater than zero.");
        if (request.Page > 1_000_000)
            return Error.Validation("Posts.PageInvalid", "Page cannot exceed 1000000.");
        if (request.PageSize is < 1 or > 100)
            return Error.Validation("Posts.PageSizeInvalid", "Page size must be between 1 and 100.");
        if (request.Q?.Trim().Length > 200)
            return Error.Validation("Posts.QueryTooLong", "Search query cannot exceed 200 characters.");
        if (request.Category?.Trim().Length > 100)
            return Error.Validation("Posts.CategoryTooLong", "Category cannot exceed 100 characters.");
        if (request.Tag?.Trim().Length > 50)
            return Error.Validation("Posts.TagTooLong", "Tag cannot exceed 50 characters.");
        if (!Enum.IsDefined(request.Sort))
            return Error.Validation("Posts.SortInvalid", "Sort must be newest, popular, oldest, or title.");

        return await repository.GetPublishedAsync(
            Clean(request.Q), Clean(request.Category), Clean(request.Tag)?.TrimStart('#'),
            request.Sort, request.Page, request.PageSize, cancellationToken);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
