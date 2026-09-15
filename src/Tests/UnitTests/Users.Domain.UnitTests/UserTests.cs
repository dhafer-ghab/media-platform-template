using Moq;
using SharedKernel.Entities.Enums;
using Users.Domain.Abstractions;

namespace Users.Domain.UnitTests;

public class UserTests
{
    // Create a mock object that simulates IPasswordHasher behavior.
    // Mocks let us control what methods return without needing a real implementation.
    private readonly Mock<IPasswordHasher> _hasherMock;

    // This is the actual interface reference we pass into our domain methods
    private readonly IPasswordHasher _hasher;

    public UserTests()
    {
        // Initialize the mock
        _hasherMock = new Mock<IPasswordHasher>();

        // Setup: When Hash() is called with ANY string,
        // return "hashed:" followed by the input password.
        // Example: Hash("Password123") → "hashed:Password123"
        _hasherMock
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns((string plainText) => $"hashed:{plainText}");

        // Setup: When Verify() is called with ANY two strings,
        // check if the hashed password matches "hashed:" + plain text.
        // Example: Verify("Password123", "hashed:Password123") → true
        _hasherMock
            .Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string plainText, string hashedPassword) =>
                hashedPassword == $"hashed:{plainText}");

        // Extract the mocked IPasswordHasher instance to use in tests.
        // _hasherMock.Object gives us an object that implements IPasswordHasher
        // with the behavior we defined above.
        _hasher = _hasherMock.Object;
    }

    [Fact]
    public void Create_WithValidInput_ReturnsSuccess()
    {
        // Act: Create a user with valid name, email, password, and role
        var result = User.Create("John Doe", "john@example.com", "Password123", Role.User, _hasher);

        // Assert: User should be created successfully with correct properties
        Assert.True(result.IsSuccess);
        Assert.Equal("John Doe", result.Value.Name);
        Assert.Equal("john@example.com", result.Value.Email.Value);
        Assert.Equal(Role.User, result.Value.Role);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ReturnsFailure(string? name)
    {
        // Act: Try to create a user with an empty or whitespace name
        var result = User.Create(name, "john@example.com", "Password123", Role.User, _hasher);

        // Assert: Should fail with "User.NameEmpty" error
        Assert.True(result.IsFailure);
        Assert.Equal("User.NameEmpty", result.Error.Code);
    }

    [Fact]
    public void Create_WithInvalidEmail_ReturnsFailure()
    {
        // Act: Try to create a user with an invalid email format
        var result = User.Create("John Doe", "not-an-email", "Password123", Role.User, _hasher);

        // Assert: Should fail with "Email.InvalidFormat" error
        Assert.True(result.IsFailure);
        Assert.Equal("Email.InvalidFormat", result.Error.Code);
    }

    [Fact]
    public void Create_WithWeakPassword_ReturnsFailure()
    {
        // Act: Try to create a user with a weak password (too short, no uppercase, etc.)
        var result = User.Create("John Doe", "john@example.com", "weak", Role.User, _hasher);

        // Assert: Should fail due to password validation
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_AssignsUniqueId()
    {
        // Act: Create two different users
        var user1 = User.Create("John Doe", "john@example.com", "Password123", Role.User, _hasher).Value;
        var user2 = User.Create("Jane Doe", "jane@example.com", "Password123", Role.User, _hasher).Value;

        // Assert: Each user should have a unique ID
        Assert.NotEqual(user1.Id, user2.Id);
    }

    [Fact]
    public void UpdateProfile_WithValidInput_UpdatesNameAndEmail()
    {
        // Arrange: Create a user first
        var user = User.Create("John Doe", "john@example.com", "Password123", Role.User, _hasher).Value;

        // Act: Update the user's profile with new name and email
        var result = user.UpdateProfile("Jane Doe", "jane@example.com");

        // Assert: Profile should be updated successfully
        Assert.True(result.IsSuccess);
        Assert.Equal("Jane Doe", user.Name);
        Assert.Equal("jane@example.com", user.Email.Value);
    }

    [Fact]
    public void UpdateProfile_WithInvalidEmail_ReturnsFailure_AndDoesNotMutateState()
    {
        // Arrange: Create a user first
        var user = User.Create("John Doe", "john@example.com", "Password123", Role.User, _hasher).Value;

        // Act: Try to update profile with an invalid email
        var result = user.UpdateProfile("Jane Doe", "not-an-email");

        // Assert: Should fail, and the original name and email should remain unchanged.
        // This ensures the method is atomic — it doesn't partially update.
        Assert.True(result.IsFailure);
        Assert.Equal("John Doe", user.Name);
        Assert.Equal("john@example.com", user.Email.Value);
    }

    [Fact]
    public void ChangePassword_WithWeakPassword_ReturnsFailure()
    {
        // Arrange: Create a user and save the original password hash
        var user = User.Create("John Doe", "john@example.com", "Password123", Role.User, _hasher).Value;
        var originalHash = user.Password.HashedValue;

        // Act: Try to change the password to a weak one
        var result = user.ChangePassword("weak", _hasher);

        // Assert: Should fail, and the password hash should remain unchanged
        Assert.True(result.IsFailure);
        Assert.Equal(originalHash, user.Password.HashedValue);
    }

    [Fact]
    public void ChangeRole_UpdatesRole()
    {
        // Arrange: Create a user with the "User" role
        var user = User.Create("John Doe", "john@example.com", "Password123", Role.User, _hasher).Value;

        // Act: Change the role to "Admin"
        user.ChangeRole(Role.Admin);

        // Assert: The role should now be "Admin"
        Assert.Equal(Role.Admin, user.Role);
    }
}