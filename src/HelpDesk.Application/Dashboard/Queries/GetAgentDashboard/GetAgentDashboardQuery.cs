using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Dashboard.Queries.GetAgentDashboard;

public sealed record GetAgentDashboardQuery(
    Guid RequestingUserId,
    UserRole RequestingUserRole);
