using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Queries.GetTickets;

public sealed record GetTicketsQuery(
    Guid RequestingUserId,
    UserRole RequestingUserRole,
    TicketStatus? StatusFilter = null,
    TicketPriority? PriorityFilter = null,
    Guid? CategoryFilter = null,
    string? SearchFilter = null,
    TicketStatus? ExcludeStatusFilter = null,
    string? ExcludeStatusesFilter = null);