using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Common;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Comments.Commands.AddComment;

public sealed class AddCommentCommandHandler
{
    private readonly ITicketRepository _tickets;
    private readonly ICommentRepository _comments;

    public AddCommentCommandHandler(ITicketRepository tickets, ICommentRepository comments)
    {
        _tickets = tickets;
        _comments = comments;
    }

    public async Task<Guid> HandleAsync(AddCommentCommand command, CancellationToken ct = default)
    {
        var ticket = await _tickets.GetByIdAsync(command.TicketId, ct)
            ?? throw new NotFoundException("Ticket not found.");

        var visible = command.RequestingUserRole switch
        {
            UserRole.Admin    => true,
            UserRole.Agent    => ticket.AssignedToId == null || ticket.AssignedToId == command.RequestingUserId,
            UserRole.Employee => ticket.CreatedById == command.RequestingUserId,
            _                 => false
        };

        if (!visible)
            throw new NotFoundException("Ticket not found.");

        var comment = new Comment(ticket.Id, command.RequestingUserId, command.Body);
        await _comments.AddAsync(comment, ct);
        return comment.Id;
    }
}