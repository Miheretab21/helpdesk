using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Dashboard.Dtos;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Dashboard.Queries.GetAdminDashboard;

public sealed class GetAdminDashboardQueryHandler
{
    private readonly ITicketRepository _tickets;
    private readonly IUserRepository _users;
    private readonly ICategoryRepository _categories;

    public GetAdminDashboardQueryHandler(
        ITicketRepository tickets,
        IUserRepository users,
        ICategoryRepository categories)
    {
        _tickets = tickets;
        _users = users;
        _categories = categories;
    }

    public async Task<AdminDashboardDto> HandleAsync(
        GetAdminDashboardQuery query,
        CancellationToken ct = default)
    {
        // Get all tickets
        var allTickets = await _tickets.ListAsync(
            status: null,
            priority: null,
            categoryId: null,
            search: null,
            excludeStatus: null,
            excludeStatuses: null,
            ct);

        // Get all users for agent names and lookup
        var allUsers = await _users.ListAsync(ct);

        // Get categories for breakdown
        var allCategories = await _categories.ListActiveAsync(ct);

        // Count by status (all statuses)
        var statusCounts = new Dictionary<int, int>();
        foreach (var status in Enum.GetValues(typeof(TicketStatus)).Cast<TicketStatus>())
        {
            statusCounts[(int)status] = allTickets.Count(t => t.Status == status);
        }

        // Breakdown by category (with category names)
        var categoryBreakdown = new Dictionary<int, int>();
        var categoryNames = allCategories.ToDictionary(c => c.Id, c => c.Name);
        
        foreach (var category in allCategories)
        {
            // Convert the index to a simple integer for grouping
            var catIndex = allCategories.ToList().IndexOf(category);
            categoryBreakdown[catIndex] = allTickets.Count(t => t.CategoryId == category.Id);
        }

        // Workload by agent (count of assigned tickets per agent)
        var workloadByAgent = allUsers
            .Where(u => u.Role == UserRole.Agent)
            .ToDictionary(
                u => u.FullName,
                u => allTickets.Count(t => t.AssignedToId == u.Id && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Cancelled));

        // Unassigned backlog
        var unassignedBacklog = allTickets.Count(t => t.AssignedToId == null);

        // Average time to close (simplified: average of (ClosedAt - CreatedAt) for closed tickets)
        var closedTickets = allTickets.Where(t => t.Status == TicketStatus.Closed).ToList();
        var averageTimeToClose = closedTickets.Any() 
            ? (decimal)closedTickets.Average(t => (t.UpdatedAt - t.CreatedAt).TotalDays)
            : 0m;

        // Recent activity (last 10 across system)
        var recentActivity = allTickets
            .OrderByDescending(t => t.UpdatedAt)
            .Take(10)
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

        // Top agents this week (resolved tickets in last 7 days)
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var topAgents = allUsers
            .Where(u => u.Role == UserRole.Agent)
            .Select(u => (
                Name: u.FullName,
                ResolvedCount: allTickets.Count(t => 
                    t.AssignedToId == u.Id && 
                    t.Status == TicketStatus.Closed && 
                    t.UpdatedAt >= weekAgo)))
            .OrderByDescending(x => x.ResolvedCount)
            .Take(5)
            .ToList();

        return new AdminDashboardDto(
            statusCounts,
            categoryBreakdown,
            workloadByAgent,
            unassignedBacklog,
            averageTimeToClose,
            recentActivity,
            topAgents);
    }
}
