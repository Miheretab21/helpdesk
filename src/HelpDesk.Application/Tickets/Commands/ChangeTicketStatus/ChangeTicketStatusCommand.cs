using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Commands.ChangeTicketStatus;

public sealed record ChangeTicketStatusCommand(
    Guid TicketId,
    TicketStatus TargetStatus,
    Guid RequestingUserId,
    UserRole RequestingUserRole);