using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Commands.CreateTicket;

public sealed record CreateTicketCommand(
    string Title,
    string Description,
    TicketPriority Priority,
    Guid CategoryId,
    Guid CreatedById);