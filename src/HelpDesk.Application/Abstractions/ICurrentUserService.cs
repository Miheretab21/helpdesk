using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Abstractions;

public interface ICurrentUserService
{
    Guid UserId { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
}