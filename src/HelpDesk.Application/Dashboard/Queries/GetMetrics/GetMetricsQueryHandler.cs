using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Dashboard.Dtos;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Dashboard.Queries.GetMetrics;

public sealed class GetMetricsQueryHandler
{
    private readonly ITicketRepository _tickets;

    public GetMetricsQueryHandler(ITicketRepository tickets)
    {
        _tickets = tickets;
    }

    public async Task<DashboardMetricsDto> HandleAsync(
        GetMetricsQuery query,
        CancellationToken ct = default)
    {
        // Get all tickets (unfiltered for dashboard)
        var allTickets = await _tickets.ListAsync(
            status: null,
            priority: null,
            categoryId: null,
            search: null,
            excludeStatus: null,
            excludeStatuses: null,
            ct);

        // Apply visibility rules based on user role
        var visibleTickets = ApplyVisibilityRule(
            allTickets,
            query.RequestingUserId,
            query.RequestingUserRole);

        // Count by status
        var statusCounts = new Dictionary<int, int>();
        foreach (var status in Enum.GetValues(typeof(TicketStatus)).Cast<TicketStatus>())
        {
            statusCounts[(int)status] = visibleTickets.Count(t => t.Status == status);
        }

        // Count by priority
        var priorityCounts = new Dictionary<int, int>();
        foreach (var priority in Enum.GetValues(typeof(TicketPriority)).Cast<TicketPriority>())
        {
            priorityCounts[(int)priority] = visibleTickets.Count(t => t.Priority == priority);
        }

        // Get recent activity (last 5 tickets updated)
        var recentActivity = visibleTickets
            .OrderByDescending(t => t.UpdatedAt)
            .Take(5)
            .Select(t => new TicketListItemDto(
                t.Id,
                t.Title,
                t.Status,
                t.Priority,
                t.AssignedToId,
                t.CreatedById,
                t.CreatedAt,
                t.UpdatedAt))
            .ToList();

        return new DashboardMetricsDto(statusCounts, priorityCounts, recentActivity);
    }

    private static IEnumerable<HelpDesk.Domain.Entities.Ticket> ApplyVisibilityRule(
        IEnumerable<HelpDesk.Domain.Entities.Ticket> tickets,
        Guid userId,
        UserRole role) => role switch
    {
        UserRole.Admin => tickets,
        UserRole.Agent => tickets.Where(t => t.AssignedToId == null || t.AssignedToId == userId),
        UserRole.Employee => tickets.Where(t => t.CreatedById == userId),
        _ => Enumerable.Empty<HelpDesk.Domain.Entities.Ticket>()
    };
}
