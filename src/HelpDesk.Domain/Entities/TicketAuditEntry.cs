using HelpDesk.Domain.Common;

namespace HelpDesk.Domain.Entities;

public class TicketAuditEntry : Entity
{
    public Guid TicketId { get; private set; }
    public Guid? ActorId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? Details { get; private set; }
    public DateTime OccurredAt { get; private set; } = DateTime.UtcNow;
    private TicketAuditEntry() { }
    public TicketAuditEntry(Guid ticketId, Guid? actorId, string action, string? details = null)
    {
        if (ticketId == Guid.Empty || string.IsNullOrWhiteSpace(action)) throw new DomainException("Ticket and action are required.");
        TicketId = ticketId; ActorId = actorId; Action = action; Details = details;
    }
}
