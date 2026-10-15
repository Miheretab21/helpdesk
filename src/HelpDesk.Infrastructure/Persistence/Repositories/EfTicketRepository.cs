using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistence.Repositories;

public sealed class EfTicketRepository : ITicketRepository
{
    private readonly AppDbContext _db;

    public EfTicketRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<Ticket>> ListAsync(
        TicketStatus? status = null,
        TicketPriority? priority = null,
        Guid? categoryId = null,
        string? search = null,
        CancellationToken ct = default)
    {
        IQueryable<Ticket> query = _db.Tickets.AsNoTracking();

        if (status is not null)
            query = query.Where(t => t.Status == status);

        if (priority is not null)
            query = query.Where(t => t.Priority == priority);

        if (categoryId is not null)
            query = query.Where(t => t.CategoryId == categoryId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLowerInvariant();
            query = query.Where(t => 
                t.Title.ToLower().Contains(searchTerm) || 
                t.Description.ToLower().Contains(searchTerm));
        }

        return await query.ToListAsync(ct);
    }

    public async Task AddAsync(Ticket ticket, CancellationToken ct = default)
    {
        await _db.Tickets.AddAsync(ticket, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Ticket ticket, CancellationToken ct = default)
    {
        _db.Tickets.Update(ticket);
        await _db.SaveChangesAsync(ct);
    }
}