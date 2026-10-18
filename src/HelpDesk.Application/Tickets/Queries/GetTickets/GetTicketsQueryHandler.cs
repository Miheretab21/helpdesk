using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Queries.GetTickets;

public sealed class GetTicketsQueryHandler
{
    private readonly ITicketRepository _tickets;

    public GetTicketsQueryHandler(ITicketRepository tickets)
    {
        _tickets = tickets;
    }

    public async Task<IReadOnlyList<TicketListItemDto>> HandleAsync(
        GetTicketsQuery query,
        CancellationToken ct = default)
    {
        var tickets = await _tickets.ListAsync(
            query.StatusFilter,
            query.PriorityFilter,
            query.CategoryFilter,
            query.SearchFilter,
            query.ExcludeStatusFilter,
            query.ExcludeStatusesFilter,
            ct);

        var visible = ApplyVisibilityRule(tickets, query.RequestingUserId, query.RequestingUserRole);

        // Narrowing filters run *after* the visibility rule so a caller can never
        // widen their reach by asking for an assignee that is out of their scope.
        if (query.UnassignedOnly == true)
            visible = visible.Where(t => t.AssignedToId is null);

        if (query.AssignedToIdFilter is not null)
            visible = visible.Where(t => t.AssignedToId == query.AssignedToIdFilter);

        return ApplySort(visible, query.SortBy, query.SortDirection)
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
    }

    private static IEnumerable<Ticket> ApplyVisibilityRule(
        IEnumerable<Ticket> tickets,
        Guid userId,
        UserRole role) => role switch
    {
        UserRole.Admin    => tickets,
        UserRole.Agent    => tickets.Where(t => t.AssignedToId == null || t.AssignedToId == userId),
        UserRole.Employee => tickets.Where(t => t.CreatedById == userId),
        _                 => Enumerable.Empty<Ticket>()
    };

    private static IEnumerable<Ticket> ApplySort(
        IEnumerable<Ticket> tickets,
        string? sortBy,
        string? sortDirection)
    {
        // Most recently updated first is the default queue order.
        var ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        return (sortBy?.Trim().ToLowerInvariant()) switch
        {
            "created"  => ascending ? tickets.OrderBy(t => t.CreatedAt)  : tickets.OrderByDescending(t => t.CreatedAt),
            "priority" => ascending ? tickets.OrderBy(t => t.Priority)   : tickets.OrderByDescending(t => t.Priority),
            "status"   => ascending ? tickets.OrderBy(t => t.Status)     : tickets.OrderByDescending(t => t.Status),
            "title"    => ascending
                ? tickets.OrderBy(t => t.Title, StringComparer.OrdinalIgnoreCase)
                : tickets.OrderByDescending(t => t.Title, StringComparer.OrdinalIgnoreCase),
            _          => ascending ? tickets.OrderBy(t => t.UpdatedAt)  : tickets.OrderByDescending(t => t.UpdatedAt),
        };
    }
}
