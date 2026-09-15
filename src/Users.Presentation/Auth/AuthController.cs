using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernal.Extensions;
using Users.Application.Users.Commands.CreateUser;
using Users.Application.Users.Commands.Login;
using SharedKernel.Entities.Enums;

namespace Users.Presentation.Auth;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request, CancellationToken ct)
    {
        var command = new CreateUserCommand(request.Name, request.Email, request.Password, Role.User);
        var result = await sender.Send(command, ct);

        return result.Match(
            id => Created($"/api/users/{id}", new { id })
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await sender.Send(command, ct);

        return result.Match(
            response => Ok(new LoginResponse(response.Token, response.UserId, response.Name, response.Email))
        );
    }
}
