using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Abstractions;

public interface ITicketAttachmentRepository
{
    Task AddAsync(TicketAttachment attachment, CancellationToken ct = default);
    Task<IReadOnlyList<TicketAttachment>> ListByTicketAsync(Guid ticketId, CancellationToken ct = default);
    Task<TicketAttachment?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
