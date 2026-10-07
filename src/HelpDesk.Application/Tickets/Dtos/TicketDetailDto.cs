using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Dtos;

public sealed record TicketDetailDto(
    Guid Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    Guid CategoryId,
    Guid? AssignedToId,
    Guid CreatedById,
    DateTime CreatedAt,
    DateTime UpdatedAt);