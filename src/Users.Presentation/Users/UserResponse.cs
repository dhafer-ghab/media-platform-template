using SharedKernel.Entities.Enums;

namespace Users.Presentation.Users;

public sealed record UserResponse(
    Guid Id,
    string Name,
    string Email,
    Role Role,
    DateTime CreatedDate,
    DateTime? UpdateDate);