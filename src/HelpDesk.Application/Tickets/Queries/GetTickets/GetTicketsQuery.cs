using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Queries.GetTickets;

public sealed record GetTicketsQuery(
    Guid RequestingUserId,
    UserRole RequestingUserRole,
    TicketStatus? StatusFilter = null,
    Guid? CategoryFilter = null);