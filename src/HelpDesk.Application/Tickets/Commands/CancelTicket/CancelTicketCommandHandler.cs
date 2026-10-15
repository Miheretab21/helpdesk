using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Common;

namespace HelpDesk.Application.Tickets.Commands.CancelTicket;

public sealed class CancelTicketCommandHandler
{
    private readonly ITicketRepository _tickets;

    public CancelTicketCommandHandler(ITicketRepository tickets)
    {
        _tickets = tickets;
    }

    public async Task HandleAsync(CancelTicketCommand command, CancellationToken ct = default)
    {
        var ticket = await _tickets.GetByIdAsync(command.TicketId, ct)
            ?? throw new NotFoundException("Ticket not found.");

        ticket.Cancel(command.RequestingUserId);

        await _tickets.UpdateAsync(ticket, ct);
    }
}