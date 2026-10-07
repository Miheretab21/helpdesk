using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Common;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Commands.AssignTicket;

public sealed class AssignTicketCommandHandler
{
    private readonly ITicketRepository _tickets;

    public AssignTicketCommandHandler(ITicketRepository tickets)
    {
        _tickets = tickets;
    }

    public async Task HandleAsync(AssignTicketCommand command, CancellationToken ct = default)
    {
        var ticket = await _tickets.GetByIdAsync(command.TicketId, ct)
            ?? throw new DomainException("Ticket not found.");

        EnsurePermissionToAssign(ticket.AssignedToId, command);

        ticket.AssignTo(command.AssignToUserId);

        await _tickets.UpdateAsync(ticket, ct);
    }

    private static void EnsurePermissionToAssign(Guid? currentAssignee, AssignTicketCommand command)
    {
        if (command.RequestingUserRole == UserRole.Admin)
            return;

        if (command.RequestingUserRole != UserRole.Agent)
            throw new DomainException("Only admins and agents can assign tickets.");

        // Agent: can only claim an unassigned ticket for themselves.
        if (currentAssignee is not null)
            throw new DomainException("This ticket is already assigned.");

        if (command.AssignToUserId != command.RequestingUserId)
            throw new DomainException("Agents can only assign tickets to themselves.");
    }
}