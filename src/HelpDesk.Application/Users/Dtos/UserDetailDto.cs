using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Users.Dtos;

public sealed record UserDetailDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    DateTime CreatedAt);
