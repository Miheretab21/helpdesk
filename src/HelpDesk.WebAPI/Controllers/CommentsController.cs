using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Comments.Commands.AddComment;
using HelpDesk.Application.Comments.Dtos;
using HelpDesk.Application.Comments.Queries.GetComments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HelpDesk.Domain.Entities;

namespace HelpDesk.WebAPI.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:guid}/comments")]
[Authorize]
public sealed class CommentsController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly GetCommentsQueryHandler _getComments;
    private readonly AddCommentCommandHandler _addComment;
    private readonly ITicketAuditRepository _audit;

    public CommentsController(
        ICurrentUserService currentUser,
        GetCommentsQueryHandler getComments,
        AddCommentCommandHandler addComment,
        ITicketAuditRepository audit)
    {
        _currentUser = currentUser;
        _getComments = getComments;
        _addComment = addComment;
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CommentDto>>> List(Guid ticketId, CancellationToken ct)
    {
        var result = await _getComments.HandleAsync(new GetCommentsQuery(
            ticketId, _currentUser.UserId, _currentUser.Role), ct);
        return Ok(result);
    }

    public sealed record AddCommentRequest(string Body);

    [HttpPost]
    public async Task<ActionResult<Guid>> Add(
        Guid ticketId,
        [FromBody] AddCommentRequest request,
        CancellationToken ct)
    {
        var id = await _addComment.HandleAsync(new AddCommentCommand(
            ticketId, request.Body, _currentUser.UserId, _currentUser.Role), ct);
        await _audit.AddAsync(new TicketAuditEntry(ticketId, _currentUser.UserId, "Comment added", "A comment was added"), ct);

        return CreatedAtAction(nameof(List), new { ticketId }, id);
    }
}
