using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Dtos;

public sealed record TicketListItemDto(
    Guid Id,
    string Title,
    TicketStatus Status,
    TicketPriority Priority,
    Guid? AssignedToId,
    Guid CreatedById,
    DateTime CreatedAt,
    DateTime UpdatedAt);