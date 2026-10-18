using HelpDesk.Domain.Common;

namespace HelpDesk.Domain.Entities;

public class TicketAttachment : Entity
{
    public Guid TicketId { get; private set; }
    public Guid UploadedById { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string StoredFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = "application/octet-stream";
    public long SizeBytes { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private TicketAttachment() { }

    public TicketAttachment(Guid ticketId, Guid uploadedById, string originalFileName, string storedFileName, string contentType, long sizeBytes)
    {
        if (ticketId == Guid.Empty || uploadedById == Guid.Empty) throw new DomainException("Ticket and uploader are required.");
        if (string.IsNullOrWhiteSpace(originalFileName) || originalFileName.Length > 255) throw new DomainException("A valid file name is required.");
        if (sizeBytes <= 0) throw new DomainException("Attachment cannot be empty.");
        TicketId = ticketId; UploadedById = uploadedById; OriginalFileName = originalFileName; StoredFileName = storedFileName;
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
        SizeBytes = sizeBytes;
    }
}
