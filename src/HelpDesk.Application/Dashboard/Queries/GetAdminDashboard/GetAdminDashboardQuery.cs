using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Dashboard.Queries.GetAdminDashboard;

public sealed record GetAdminDashboardQuery(
    Guid RequestingUserId,
    UserRole RequestingUserRole);
