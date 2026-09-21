using Posts.Application.Abstractions;
using SharedKernal.Messaging;
using SharedKernal.Results;

namespace Posts.Application.Posts.Commands.RecordPostView;

internal sealed class RecordPostViewCommandHandler(IPostRepository repository)
    : ICommandHandler<RecordPostViewCommand, Result>
{
    public async Task<Result> Handle(RecordPostViewCommand request, CancellationToken cancellationToken)
    {
        var updated = await repository.IncrementPublishedViewCountAsync(request.Id, cancellationToken);
        return updated
            ? Result.Success()
            : Result.Failure(Error.NotFound("Post.NotFound", $"Published post with id {request.Id} was not found."));
    }
}
