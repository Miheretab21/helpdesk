using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Abstractions;

public interface ITicketAuditRepository
{
    Task AddAsync(TicketAuditEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<TicketAuditEntry>> ListByTicketAsync(Guid ticketId, CancellationToken ct = default);
}
