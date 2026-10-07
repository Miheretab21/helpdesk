using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Comments.Queries.GetComments;

public sealed record GetCommentsQuery(
    Guid TicketId,
    Guid RequestingUserId,
    UserRole RequestingUserRole);