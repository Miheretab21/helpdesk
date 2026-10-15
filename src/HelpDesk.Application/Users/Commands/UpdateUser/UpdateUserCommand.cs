using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string? FullName,
    UserRole? Role,
    string? Password);
