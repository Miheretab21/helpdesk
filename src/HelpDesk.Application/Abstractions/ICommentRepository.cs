using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Abstractions;

public interface ICommentRepository
{
    Task<IReadOnlyList<Comment>> ListByTicketAsync(Guid ticketId, CancellationToken ct = default);
    Task AddAsync(Comment comment, CancellationToken ct = default);
}