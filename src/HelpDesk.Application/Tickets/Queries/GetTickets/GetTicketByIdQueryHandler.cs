using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Common;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Queries.GetTicketById;

public sealed class GetTicketByIdQueryHandler
{
    private readonly ITicketRepository _tickets;

    public GetTicketByIdQueryHandler(ITicketRepository tickets)
    {
        _tickets = tickets;
    }

    public async Task<TicketListItemDto> HandleAsync(
        GetTicketByIdQuery query,
        CancellationToken ct = default)
    {
        var ticket = await _tickets.GetByIdAsync(query.TicketId, ct)
            ?? throw new DomainException("Ticket not found.");

        if (!IsVisible(ticket, query.RequestingUserId, query.RequestingUserRole))
            throw new DomainException("Ticket not found.");

        return new TicketListItemDto(
            ticket.Id, ticket.Title, ticket.Status, ticket.Priority,
            ticket.AssignedToId, ticket.CreatedById, ticket.CreatedAt, ticket.UpdatedAt);
    }

    private static bool IsVisible(Domain.Entities.Ticket ticket, Guid userId, UserRole role) => role switch
    {
        UserRole.Admin    => true,
        UserRole.Agent    => ticket.AssignedToId == null || ticket.AssignedToId == userId,
        UserRole.Employee => ticket.CreatedById == userId,
        _                 => false
    };
}