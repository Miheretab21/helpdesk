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
        var tickets = await _tickets.ListAsync(query.StatusFilter, query.CategoryFilter, ct);

        var visible = ApplyVisibilityRule(tickets, query.RequestingUserId, query.RequestingUserRole);

        return visible
            .OrderByDescending(t => t.UpdatedAt)
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
}