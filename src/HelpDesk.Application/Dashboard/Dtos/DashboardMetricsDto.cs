using HelpDesk.Application.Tickets.Dtos;

namespace HelpDesk.Application.Dashboard.Dtos;

public sealed record DashboardMetricsDto(
    Dictionary<int, int> TicketCountByStatus,
    Dictionary<int, int> TicketCountByPriority,
    IReadOnlyList<TicketListItemDto> RecentActivity);

public sealed record EmployeeDashboardDto(
    Dictionary<int, int> MyTicketsByStatus,
    int ResolvedToday,
    IReadOnlyList<TicketListItemDto> MyRecentActivity);

public sealed record AgentDashboardDto(
    Dictionary<int, int> MyTicketsByStatus,
    int UnassignedQueueCount,
    IReadOnlyList<TicketListItemDto> MyRecentActivity);

public sealed record AdminDashboardDto(
    Dictionary<int, int> AllTicketsByStatus,
    Dictionary<int, int> TicketsByCategory,
    Dictionary<string, int> WorkloadByAgent,
    int UnassignedBacklog,
    decimal AverageTimeToClose,
    IReadOnlyList<TicketListItemDto> RecentActivitySystem,
    IReadOnlyList<(string AgentName, int ResolvedCount)> TopAgentsThisWeek);
