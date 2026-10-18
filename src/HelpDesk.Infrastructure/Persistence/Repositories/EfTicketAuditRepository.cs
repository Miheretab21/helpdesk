using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistence.Repositories;

public sealed class EfTicketAuditRepository(AppDbContext db) : ITicketAuditRepository
{
    public async Task AddAsync(TicketAuditEntry entry, CancellationToken ct = default) { await db.TicketAuditEntries.AddAsync(entry, ct); await db.SaveChangesAsync(ct); }
    public async Task<IReadOnlyList<TicketAuditEntry>> ListByTicketAsync(Guid ticketId, CancellationToken ct = default) => await db.TicketAuditEntries.AsNoTracking().Where(x => x.TicketId == ticketId).OrderByDescending(x => x.OccurredAt).ToListAsync(ct);
}
