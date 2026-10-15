using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Dashboard.Queries.GetMetrics;

public sealed record GetMetricsQuery(
    Guid RequestingUserId,
    UserRole RequestingUserRole);
