using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistence.Repositories;

public sealed class EfTicketAttachmentRepository(AppDbContext db) : ITicketAttachmentRepository
{
    public async Task AddAsync(TicketAttachment attachment, CancellationToken ct = default) { await db.TicketAttachments.AddAsync(attachment, ct); await db.SaveChangesAsync(ct); }
    public async Task<IReadOnlyList<TicketAttachment>> ListByTicketAsync(Guid ticketId, CancellationToken ct = default) => await db.TicketAttachments.AsNoTracking().Where(x => x.TicketId == ticketId).OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
    public Task<TicketAttachment?> GetByIdAsync(Guid id, CancellationToken ct = default) => db.TicketAttachments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
}
