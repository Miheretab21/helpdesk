using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Common;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Commands.EditTicket;

public sealed class EditTicketCommandHandler
{
    private readonly ITicketRepository _tickets;

    public EditTicketCommandHandler(ITicketRepository tickets)
    {
        _tickets = tickets;
    }

    public async Task HandleAsync(EditTicketCommand command, CancellationToken ct = default)
    {
        if (command.RequestingUserRole != UserRole.Admin)
            throw new DomainException("Only admins can edit tickets.");

        var ticket = await _tickets.GetByIdAsync(command.TicketId, ct)
            ?? throw new NotFoundException("Ticket not found.");

        ticket.Edit(command.Title, command.Description, command.CategoryId);

        await _tickets.UpdateAsync(ticket, ct);
    }
}