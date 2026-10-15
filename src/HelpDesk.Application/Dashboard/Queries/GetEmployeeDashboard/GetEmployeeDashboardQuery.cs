using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Dashboard.Queries.GetEmployeeDashboard;

public sealed record GetEmployeeDashboardQuery(
    Guid RequestingUserId,
    UserRole RequestingUserRole);
