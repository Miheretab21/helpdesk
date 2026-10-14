using System.Security.Claims;
using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Enums;

namespace HelpDesk.WebAPI.Services;

public sealed class JwtCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
    }

    public UserRole Role
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(claim, ignoreCase: true, out var role)
                ? role
                : UserRole.Employee;
        }
    }

    public bool IsAuthenticated => UserId != Guid.Empty;
}