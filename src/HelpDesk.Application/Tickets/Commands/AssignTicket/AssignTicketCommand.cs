using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Commands.AssignTicket;

public sealed record AssignTicketCommand(
    Guid TicketId,
    Guid AssignToUserId,
    Guid RequestingUserId,
    UserRole RequestingUserRole);