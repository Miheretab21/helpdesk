using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Queries.GetTicketById;

public sealed record GetTicketByIdQuery(
    Guid TicketId,
    Guid RequestingUserId,
    UserRole RequestingUserRole);