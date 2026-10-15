using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Users.Dtos;

public sealed record UserListItemDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role);
