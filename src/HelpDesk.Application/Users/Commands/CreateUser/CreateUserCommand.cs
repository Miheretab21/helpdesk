using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string FullName,
    string Password,
    UserRole Role);
