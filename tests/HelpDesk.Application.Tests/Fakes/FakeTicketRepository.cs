using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tests.Fakes;

internal sealed class FakeTicketRepository : ITicketRepository
{
    private readonly List<Ticket> _tickets;

    public FakeTicketRepository(IEnumerable<Ticket>? tickets = null)
    {
        _tickets = tickets?.ToList() ?? new List<Ticket>();
    }

    public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_tickets.FirstOrDefault(t => t.Id == id));

    public Task<IReadOnlyList<Ticket>> ListAsync(
        TicketStatus? status = null,
        TicketPriority? priority = null,
        Guid? categoryId = null,
        string? search = null,
        CancellationToken ct = default)
    {
        IEnumerable<Ticket> q = _tickets;
        if (status is not null) q = q.Where(t => t.Status == status);
        if (priority is not null) q = q.Where(t => t.Priority == priority);
        if (categoryId is not null) q = q.Where(t => t.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLowerInvariant();
            q = q.Where(t => t.Title.ToLower().Contains(searchTerm) || t.Description.ToLower().Contains(searchTerm));
        }
        return Task.FromResult<IReadOnlyList<Ticket>>(q.ToList());
    }

    public Task AddAsync(Ticket ticket, CancellationToken ct = default)
    {
        _tickets.Add(ticket);
        return Task.CompletedTask;
    }
    public Task UpdateAsync(Ticket ticket, CancellationToken ct = default)
{
    var index = _tickets.FindIndex(t => t.Id == ticket.Id);
    if (index >= 0) _tickets[index] = ticket;
    return Task.CompletedTask;
}
}