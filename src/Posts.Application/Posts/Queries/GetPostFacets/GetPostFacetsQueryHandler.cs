using Posts.Application.Abstractions;
using SharedKernal.Messaging;

namespace Posts.Application.Posts.Queries.GetPostFacets;

internal sealed class GetPostFacetsQueryHandler(IPostRepository repository)
    : IQueryHandler<GetPostFacetsQuery, PostFacetsDto>
{
    public Task<PostFacetsDto> Handle(GetPostFacetsQuery request, CancellationToken cancellationToken) =>
        repository.GetPublishedFacetsAsync(cancellationToken);
}
