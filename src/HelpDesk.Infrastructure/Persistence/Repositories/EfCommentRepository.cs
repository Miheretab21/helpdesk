using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistence.Repositories;

public sealed class EfCommentRepository : ICommentRepository
{
    private readonly AppDbContext _db;

    public EfCommentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Comment>> ListByTicketAsync(Guid ticketId, CancellationToken ct = default) =>
        await _db.Comments
            .AsNoTracking()
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Comment comment, CancellationToken ct = default)
    {
        await _db.Comments.AddAsync(comment, ct);
        await _db.SaveChangesAsync(ct);
    }
}