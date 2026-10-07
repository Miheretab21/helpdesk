using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Abstractions;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> ListActiveAsync(CancellationToken ct = default);
}