namespace HelpDesk.Application.Comments.Dtos;

public sealed record CommentDto(
    Guid Id,
    Guid TicketId,
    Guid AuthorId,
    string Body,
    DateTime CreatedAt);