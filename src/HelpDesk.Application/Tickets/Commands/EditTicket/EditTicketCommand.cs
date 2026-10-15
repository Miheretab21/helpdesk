using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Commands.EditTicket;

public sealed record EditTicketCommand(
    Guid TicketId,
    string Title,
    string Description,
    Guid CategoryId,
    Guid RequestingUserId,
    UserRole RequestingUserRole);