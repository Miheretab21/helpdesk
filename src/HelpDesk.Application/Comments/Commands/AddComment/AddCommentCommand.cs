using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Comments.Commands.AddComment;

public sealed record AddCommentCommand(
    Guid TicketId,
    string Body,
    Guid RequestingUserId,
    UserRole RequestingUserRole);