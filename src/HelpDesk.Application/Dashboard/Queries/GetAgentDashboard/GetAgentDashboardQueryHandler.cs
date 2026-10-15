using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Dashboard.Dtos;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Dashboard.Queries.GetAgentDashboard;

public sealed class GetAgentDashboardQueryHandler
{
    private readonly ITicketRepository _tickets;

    public GetAgentDashboardQueryHandler(ITicketRepository tickets)
    {
        _tickets = tickets;
    }

    public async Task<AgentDashboardDto> HandleAsync(
        GetAgentDashboardQuery query,
        CancellationToken ct = default)
    {
        // Get all tickets (unfiltered)
        var allTickets = await _tickets.ListAsync(
            status: null,
            priority: null,
            categoryId: null,
            search: null,
            excludeStatus: null,
            excludeStatuses: null,
            ct);

        // My assigned tickets
        var myTickets = allTickets.Where(t => t.AssignedToId == query.RequestingUserId).ToList();

        // Count my tickets by status (only relevant statuses for agent)
        var statusCounts = new Dictionary<int, int>
        {
            [(int)TicketStatus.Assigned] = myTickets.Count(t => t.Status == TicketStatus.Assigned),
            [(int)TicketStatus.InProgress] = myTickets.Count(t => t.Status == TicketStatus.InProgress),
            [(int)TicketStatus.Resolved] = myTickets.Count(t => t.Status == TicketStatus.Resolved),
        };

        // Unassigned queue (New tickets with no assignment)
        var unassignedCount = allTickets.Count(t => 
            t.Status == TicketStatus.New && t.AssignedToId == null);

        // My recent activity
        var myRecentActivity = myTickets
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

        return new AgentDashboardDto(statusCounts, unassignedCount, myRecentActivity);
    }
}
