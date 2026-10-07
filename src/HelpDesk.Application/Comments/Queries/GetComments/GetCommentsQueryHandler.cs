using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Comments.Dtos;
using HelpDesk.Domain.Common;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Comments.Queries.GetComments;

public sealed class GetCommentsQueryHandler
{
    private readonly ITicketRepository _tickets;
    private readonly ICommentRepository _comments;

    public GetCommentsQueryHandler(ITicketRepository tickets, ICommentRepository comments)
    {
        _tickets = tickets;
        _comments = comments;
    }

    public async Task<IReadOnlyList<CommentDto>> HandleAsync(
        GetCommentsQuery query,
        CancellationToken ct = default)
    {
        var ticket = await _tickets.GetByIdAsync(query.TicketId, ct)
            ?? throw new NotFoundException("Ticket not found.");

        // Visibility rule — same as GetTicketById.
        var visible = query.RequestingUserRole switch
        {
            UserRole.Admin    => true,
            UserRole.Agent    => ticket.AssignedToId == null || ticket.AssignedToId == query.RequestingUserId,
            UserRole.Employee => ticket.CreatedById == query.RequestingUserId,
            _                 => false
        };

        if (!visible)
            throw new NotFoundException("Ticket not found.");

        var comments = await _comments.ListByTicketAsync(query.TicketId, ct);
        return comments
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(c.Id, c.TicketId, c.AuthorId, c.Body, c.CreatedAt))
            .ToList();
    }
}