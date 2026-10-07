using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Common;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Commands.ChangeTicketStatus;

public sealed class ChangeTicketStatusCommandHandler
{
    private readonly ITicketRepository _tickets;

    public ChangeTicketStatusCommandHandler(ITicketRepository tickets)
    {
        _tickets = tickets;
    }

    public async Task HandleAsync(ChangeTicketStatusCommand command, CancellationToken ct = default)
    {
        var ticket = await _tickets.GetByIdAsync(command.TicketId, ct)
            ?? throw new DomainException("Ticket not found.");

        EnsurePermissionToChangeStatus(command);

        // The domain decides whether the transition is legal.
        switch (command.TargetStatus)
        {
            case TicketStatus.InProgress:
                ticket.StartWork(command.RequestingUserId);
                break;
            case TicketStatus.Resolved:
                ticket.Resolve(command.RequestingUserId);
                break;
            case TicketStatus.Closed:
                ticket.Close(command.RequestingUserId);
                break;
            default:
                throw new DomainException(
                    $"Cannot transition a ticket to '{command.TargetStatus}' via this command.");
        }

        await _tickets.UpdateAsync(ticket, ct);
    }

    private static void EnsurePermissionToChangeStatus(ChangeTicketStatusCommand command)
    {
        if (command.RequestingUserRole == UserRole.Admin || command.RequestingUserRole == UserRole.Agent)
            return;

        throw new DomainException("Only agents and admins can change a ticket's status.");
    }
}