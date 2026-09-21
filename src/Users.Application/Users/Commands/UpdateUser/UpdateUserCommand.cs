using SharedKernal.Messaging;
using SharedKernal.Results;
using SharedKernel.Entities.Enums;

namespace Users.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string Name,
    string Email,
    string Password,
    Role Role) : ICommand<Result<UserDto>>;
