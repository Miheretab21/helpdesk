using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Dashboard.Dtos;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Dashboard.Queries.GetEmployeeDashboard;

public sealed class GetEmployeeDashboardQueryHandler
{
    private readonly ITicketRepository _tickets;

    public GetEmployeeDashboardQueryHandler(ITicketRepository tickets)
    {
        _tickets = tickets;
    }

    public async Task<EmployeeDashboardDto> HandleAsync(
        GetEmployeeDashboardQuery query,
        CancellationToken ct = default)
    {
        // Get all tickets created by this employee
        var allTickets = await _tickets.ListAsync(
            status: null,
            priority: null,
            categoryId: null,
            search: null,
            excludeStatus: null,
            excludeStatuses: null,
            ct);

        // Filter to only their tickets
        var myTickets = allTickets.Where(t => t.CreatedById == query.RequestingUserId).ToList();

        // Count by status
        var statusCounts = new Dictionary<int, int>();
        foreach (var status in Enum.GetValues(typeof(TicketStatus)).Cast<TicketStatus>())
        {
            statusCounts[(int)status] = myTickets.Count(t => t.Status == status);
        }

        // Tickets that reached Resolved during the current day.
        var today = DateTime.UtcNow.Date;
        var resolvedToday = myTickets.Count(t =>
            t.Status == TicketStatus.Resolved && t.UpdatedAt.Date == today);

        // Get recent activity (last 5 of their tickets updated)
        var recentActivity = myTickets
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
                t.UpdatedAt,
                t.SlaDueAt,
                t.EscalatedAt))
            .ToList();

        return new EmployeeDashboardDto(statusCounts, resolvedToday, recentActivity);
    }
}
