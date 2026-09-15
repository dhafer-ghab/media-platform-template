using SharedKernal.Messaging;
using SharedKernal.Results;
using Users.Application.Abstractions;
using Users.Domain.Abstractions;

namespace Users.Application.Users.Commands.Login;

internal sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
    : ICommandHandler<LoginCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            // timing attack protection to prevent user enumeration
            passwordHasher.PerformFakeVerification();
            return Error.NotFound("User.InvalidCredentials", "Email or password is incorrect.");
        }
            

        var passwordValid = passwordHasher.Verify(request.Password, user.Password.HashedValue);
        if (!passwordValid)
            return Error.NotFound("User.InvalidCredentials", "Email or password is incorrect.");

        var token = tokenService.GenerateToken(user);

        return new LoginResponseDto(token, user.Id, user.Name, user.Email.Value);
    }
}