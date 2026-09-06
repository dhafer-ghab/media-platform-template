using MassTransit;
using SharedKernal.Messaging;
using SharedKernal.Results;
using Users.Application.Abstractions;

namespace Users.Application.Users.Commands.DeleteUser;

internal sealed class DeleteUserCommandHandler(IUserRepository userRepository)
    : ICommandHandler<DeleteUserCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
        {
            return Error.NotFound(ErrorCodes.NotFound, $"user with {request.Id} not found");
        }

        user.Delete();
        userRepository.Remove(user);
        var saveResult = await userRepository.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;

        return true;
    }
}
