using SharedKernal.Messaging;
using SharedKernel.Entities.Enums;
using SharedKernal.Results;

namespace Users.Application.Users.Commands.CreateUser;

internal sealed record CreateUserCommand(
    string Name,
    string Email,
    string Password,
    Role Role) : ICommand<Result<Guid>>;
