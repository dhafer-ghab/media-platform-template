using SharedKernal.Entities;
using SharedKernal.Results;
using SharedKernel.Entities.Enums;
using Users.Domain.Abstractions;
using Users.Domain.DomainEvents;
using Users.Domain.ValueObjects;

namespace Users.Domain;

public class User : AggregateRoot
{
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public Password Password { get; private set; }
    public Role Role { get; private set; }

    private User(Guid id, string name, Email email, Password password, Role role)
        : base(id)
    {
        Name = name;
        Email = email;
        Password = password;
        Role = role;
    }

    private User()
    {
        Name = null!;
        Email = null!;
        Password = null!;
    }

    public static Result<User> Create(string? name, string? email, string? plainTextPassword, Role role, IPasswordHasher hasher)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<User>(Error.Validation("User.NameEmpty", "Name cannot be empty."));

        if (name.Length > 200)
            return Result.Failure<User>(Error.Validation("User.NameTooLong", "Name must not exceed 200 characters."));

        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
            return Result.Failure<User>(emailResult.Errors);

        var passwordResult = Password.Create(plainTextPassword, hasher);
        if (passwordResult.IsFailure)
            return Result.Failure<User>(passwordResult.Errors);

        var user = new User(Guid.NewGuid(), name.Trim(), emailResult.Value, passwordResult.Value, role);
        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id, user.Name, user.Email.Value));
        return Result.Success(user);
    }

    public Result UpdateProfile(string? name, string? email)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("User.NameEmpty", "Name cannot be empty."));

        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
            return Result.Failure(emailResult.Errors);

        Name = name.Trim();
        Email = emailResult.Value;
        UpdateDate = DateTime.UtcNow;
        RaiseDomainEvent(new UserUpdatedDomainEvent(Id, Name, Email.Value));
        return Result.Success();
    }

    public Result ChangePassword(string? newPlainTextPassword, IPasswordHasher hasher)
    {
        var passwordResult = Password.Create(newPlainTextPassword, hasher);
        if (passwordResult.IsFailure)
            return Result.Failure(passwordResult.Errors);

        Password = passwordResult.Value;
        UpdateDate = DateTime.UtcNow;
        return Result.Success();
    }

    public void Delete()
    {
        RaiseDomainEvent(new UserDeletedDomainEvent(Id));
    }

    public Result ChangeRole(Role newRole)
    {
        Role = newRole;
        UpdateDate = DateTime.UtcNow;
        return Result.Success();
    }
}