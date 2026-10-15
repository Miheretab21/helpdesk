namespace HelpDesk.Application.Tickets.Commands.CancelTicket;

public sealed record CancelTicketCommand(
    Guid TicketId,
    Guid RequestingUserId);