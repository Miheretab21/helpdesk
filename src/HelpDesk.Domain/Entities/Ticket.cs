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
    public DateTime SlaDueAt { get; private set; }
    public DateTime? EscalatedAt { get; private set; }

    // EF Core needs a parameterless constructor to materialize from the DB.
    private Ticket() { }

    public Ticket(string title, string description, TicketPriority priority, Guid categoryId, Guid createdById)
    {
        ValidateTitle(title);
        ValidateDescription(description);

        Title = title.Trim();
        Description = description.Trim();
        Priority = priority;
        CategoryId = categoryId;
        CreatedById = createdById;
        Status = TicketStatus.New;
        SlaDueAt = DateTime.UtcNow.Add(GetSlaTarget(priority));
    }

    public void Edit(string title, string description, Guid categoryId)
    {
        ValidateTitle(title);
        ValidateDescription(description);
        if (categoryId == Guid.Empty)
            throw new DomainException("Category is required.");

        Title = title.Trim();
        Description = description.Trim();
        CategoryId = categoryId;
        Touch();
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

    public void Cancel(Guid byUserId)
    {
        if (CreatedById != byUserId)
            throw new DomainException("Only the ticket's creator can cancel it.");
        if (Status != TicketStatus.New)
            throw new DomainException("Only a new ticket can be cancelled.");

        Status = TicketStatus.Cancelled;
        Touch();
    }

    public void StartWork(Guid byUserId, bool isAdmin = false)
    {
        EnsureCanAct(byUserId, isAdmin);
        if (Status != TicketStatus.Assigned)
            throw new DomainException("Only an assigned ticket can be started.");
        Status = TicketStatus.InProgress;
        Touch();
    }

    public void Resolve(Guid byUserId, bool isAdmin = false)
    {
        EnsureCanAct(byUserId, isAdmin);
        if (Status != TicketStatus.InProgress)
            throw new DomainException("Only an in-progress ticket can be resolved.");
        Status = TicketStatus.Resolved;
        Touch();
    }

    public void Close(Guid byUserId, bool isAdmin = false)
    {
        EnsureCanAct(byUserId, isAdmin);
        if (Status != TicketStatus.Resolved)
            throw new DomainException("Only a resolved ticket can be closed.");
        Status = TicketStatus.Closed;
        Touch();
    }

    public void Reopen(Guid byUserId, bool isAdmin = false)
    {
        if (CreatedById != byUserId && !isAdmin)
            throw new DomainException("Only the ticket creator or an admin can reopen a closed ticket.");
        if (Status != TicketStatus.Closed)
            throw new DomainException("Only a closed ticket can be reopened.");

        Status = AssignedToId is null ? TicketStatus.New : TicketStatus.Assigned;
        Touch();
    }

    public void Escalate()
    {
        if (Status is TicketStatus.Closed or TicketStatus.Cancelled)
            throw new DomainException("Closed or cancelled tickets cannot be escalated.");
        EscalatedAt ??= DateTime.UtcNow;
        Touch();
    }

    // ---------------------------------------------------------------------
    // Private helpers
    // ---------------------------------------------------------------------

    private void EnsureCanAct(Guid userId, bool isAdmin)
    {
        // Admins have override authority — they can act on any ticket.
        if (isAdmin) return;

        if (AssignedToId != userId)
            throw new DomainException("Only the assigned agent can perform this action.");
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");
        if (title.Length > 200)
            throw new DomainException("Title cannot exceed 200 characters.");
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");
        if (description.Length > 4000)
            throw new DomainException("Description cannot exceed 4000 characters.");
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;

    private static TimeSpan GetSlaTarget(TicketPriority priority) => priority switch
    {
        TicketPriority.Urgent => TimeSpan.FromHours(4),
        TicketPriority.High => TimeSpan.FromHours(8),
        TicketPriority.Medium => TimeSpan.FromHours(24),
        _ => TimeSpan.FromHours(72)
    };
}
