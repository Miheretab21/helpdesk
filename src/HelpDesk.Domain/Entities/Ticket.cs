using HelpDesk.Domain.Common;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Domain.Entities;

public class Ticket : Entity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TicketStatus Status { get; private set; } = TicketStatus.New;
    public TicketPriority Priority { get; private set; }

    public Guid CategoryId { get; private set; }
    public Guid CreatedById { get; private set; }
    public Guid? AssignedToId { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // EF Core needs a parameterless constructor to materialize from the DB.
    private Ticket() { }

    public Ticket(string title, string description, TicketPriority priority, Guid categoryId, Guid createdById)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");
        if (title.Length > 200)
            throw new DomainException("Title cannot exceed 200 characters.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");
        if (description.Length > 4000)
            throw new DomainException("Description cannot exceed 4000 characters.");

        Title = title.Trim();
        Description = description.Trim();
        Priority = priority;
        CategoryId = categoryId;
        CreatedById = createdById;
        Status = TicketStatus.New;
    }

    public void AssignTo(Guid agentId)
    {
        if (Status != TicketStatus.New)
            throw new DomainException("Only a new ticket can be assigned.");
        if (agentId == Guid.Empty)
            throw new DomainException("Agent id is required.");

        AssignedToId = agentId;
        Status = TicketStatus.Assigned;
        Touch();
    }

    public void StartWork(Guid byAgentId)
    {
        EnsureAssignedTo(byAgentId);
        if (Status != TicketStatus.Assigned)
            throw new DomainException("Only an assigned ticket can be started.");
        Status = TicketStatus.InProgress;
        Touch();
    }

    public void Resolve(Guid byAgentId)
    {
        EnsureAssignedTo(byAgentId);
        if (Status != TicketStatus.InProgress)
            throw new DomainException("Only an in-progress ticket can be resolved.");
        Status = TicketStatus.Resolved;
        Touch();
    }

    public void Close(Guid byAgentId)
    {
        EnsureAssignedTo(byAgentId);
        if (Status != TicketStatus.Resolved)
            throw new DomainException("Only a resolved ticket can be closed.");
        Status = TicketStatus.Closed;
        Touch();
    }

    private void EnsureAssignedTo(Guid agentId)
    {
        if (AssignedToId != agentId)
            throw new DomainException("Only the assigned agent can perform this action.");
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}