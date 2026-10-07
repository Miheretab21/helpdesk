using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Tickets.Commands.CreateTicket;

public sealed class CreateTicketCommandHandler
{
    private readonly ITicketRepository _tickets;

    public CreateTicketCommandHandler(ITicketRepository tickets)
    {
        _tickets = tickets;
    }

    public async Task<Guid> HandleAsync(CreateTicketCommand command, CancellationToken ct = default)
    {
        var ticket = new Ticket(
            command.Title,
            command.Description,
            command.Priority,
            command.CategoryId,
            command.CreatedById);

        await _tickets.AddAsync(ticket, ct);

        return ticket.Id;
    }
}