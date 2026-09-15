using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SharedKernal.Results;
using Users.Application.Users.Commands.CreateUser;
using Users.Application.Users.Commands.Login;
using SharedKernel.Entities.Enums;
using Users.Presentation.Auth;

namespace Users.Presentation.UnitTests;

public class AuthControllerTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _senderMock = new Mock<ISender>();
        _controller = new AuthController(_senderMock.Object);
    }

    [Fact]
    public async Task Signup_WithValidRequest_ReturnsCreatedWithUserLocation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SignupRequest("John Doe", "john@example.com", "Password123");

        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(userId));

        // Act
        var result = await _controller.Signup(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal($"/api/users/{userId}", createdResult.Location);

        var value = createdResult.Value!;
        var idProperty = value.GetType().GetProperty("id");
        Assert.NotNull(idProperty);
        Assert.Equal(userId, idProperty.GetValue(value));
    }

    [Fact]
    public async Task Signup_WithValidationFailure_ReturnsBadRequest()
    {
        // Arrange
        var request = new SignupRequest("", "john@example.com", "Password123");
        var error = Error.Validation("User.NameEmpty", "Name is required.");

        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Guid>(error));

        // Act
        var result = await _controller.Signup(request, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithLoginResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new LoginRequest("john@example.com", "Password123");
        var responseDto = new LoginResponseDto("test-token", userId, "John Doe", "john@example.com");

        _senderMock
            .Setup(s => s.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(responseDto));

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal("test-token", response.Token);
        Assert.Equal(userId, response.UserId);
        Assert.Equal("John Doe", response.Name);
        Assert.Equal("john@example.com", response.Email);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsNotFound()
    {
        // Arrange
        var request = new LoginRequest("john@example.com", "wrong-password");
        var error = Error.NotFound("User.InvalidCredentials", "Email or password is incorrect.");

        _senderMock
            .Setup(s => s.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<LoginResponseDto>(error));

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidationFailure_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest("not-an-email", "Password123");
        var error = Error.Validation("Email.InvalidFormat", "Invalid email format.");

        _senderMock
            .Setup(s => s.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<LoginResponseDto>(error));

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }
}
