using SharedKernal.Messaging;
using SharedKernal.Results;

namespace Posts.Application.Posts.Commands.RecordPostView;

public sealed record RecordPostViewCommand(Guid Id) : ICommand<Result>;
