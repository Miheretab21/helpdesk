using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Abstractions;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Ticket>> ListAsync(
        TicketStatus? status = null,
        TicketPriority? priority = null,
        Guid? categoryId = null,
        string? search = null,
        TicketStatus? excludeStatus = null,
        string? excludeStatuses = null,
        CancellationToken ct = default);

    Task AddAsync(Ticket ticket, CancellationToken ct = default);
    Task UpdateAsync(Ticket ticket, CancellationToken ct = default);
}