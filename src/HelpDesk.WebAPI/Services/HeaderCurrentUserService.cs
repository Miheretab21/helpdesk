using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Enums;

namespace HelpDesk.WebAPI.Services;

public sealed class HeaderCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var header = _httpContextAccessor.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault();
            return Guid.TryParse(header, out var id) ? id : Guid.Empty;
        }
    }

    public UserRole Role
    {
        get
        {
            var header = _httpContextAccessor.HttpContext?.Request.Headers["X-User-Role"].FirstOrDefault();
            return Enum.TryParse<UserRole>(header, ignoreCase: true, out var role)
                ? role
                : UserRole.Employee;
        }
    }

    public bool IsAuthenticated => UserId != Guid.Empty;
}