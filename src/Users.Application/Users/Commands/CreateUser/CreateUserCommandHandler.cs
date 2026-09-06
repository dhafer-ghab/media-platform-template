using SharedKernal.Messaging;
using Users.Application.Abstractions;
using Users.Domain;
using Users.Domain.Abstractions;
using SharedKernal.Results;
namespace Users.Application.Users.Commands.CreateUser;

internal sealed class CreateUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    : ICommandHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email?.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedEmail) &&
            await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken) is not null)
            return Error.Conflict("User.EmailExists", "A user with that email already exists.");

        Result<User> result = User.Create(request.Name, request.Email, request.Password, request.Role, passwordHasher);

        if (result.IsFailure)
            return result.Error;

        var user = result.Value;
        await userRepository.AddAsync(user, cancellationToken);

        var saveResult = await userRepository.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;

        return user.Id;
    }
}
