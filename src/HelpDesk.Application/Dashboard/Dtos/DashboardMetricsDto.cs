using HelpDesk.Application.Tickets.Dtos;

namespace HelpDesk.Application.Dashboard.Dtos;

public sealed record DashboardMetricsDto(
    Dictionary<int, int> TicketCountByStatus,
    Dictionary<int, int> TicketCountByPriority,
    IReadOnlyList<TicketListItemDto> RecentActivity);
