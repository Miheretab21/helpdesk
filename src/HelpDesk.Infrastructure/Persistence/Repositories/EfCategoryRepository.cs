using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistence.Repositories;

public sealed class EfCategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;

    public EfCategoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Category>> ListActiveAsync(CancellationToken ct = default) =>
        await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
}