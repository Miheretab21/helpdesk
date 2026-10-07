using HelpDesk.Domain.Common;

namespace HelpDesk.Domain.Entities;

public class Comment : Entity
{
    public Guid TicketId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Comment() { }

    public Comment(Guid ticketId, Guid authorId, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("Comment body is required.");
        TicketId = ticketId;
        AuthorId = authorId;
        Body = body.Trim();
    }
}