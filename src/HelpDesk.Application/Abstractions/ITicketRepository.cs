using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Abstractions;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Ticket>> ListAsync(
        TicketStatus? status,
        Guid? categoryId,
        CancellationToken ct = default);

    Task AddAsync(Ticket ticket, CancellationToken ct = default);
}