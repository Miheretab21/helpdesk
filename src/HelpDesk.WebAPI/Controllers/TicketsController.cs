using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Tickets.Commands.AssignTicket;
using HelpDesk.Application.Tickets.Commands.ChangeTicketStatus;
using HelpDesk.Application.Tickets.Commands.CreateTicket;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Application.Tickets.Queries.GetTicketById;
using HelpDesk.Application.Tickets.Queries.GetTickets;
using HelpDesk.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.WebAPI.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly GetTicketsQueryHandler _getTickets;
    private readonly GetTicketByIdQueryHandler _getTicketById;
    private readonly CreateTicketCommandHandler _createTicket;
    private readonly AssignTicketCommandHandler _assignTicket;
    private readonly ChangeTicketStatusCommandHandler _changeStatus;

    public TicketsController(
        ICurrentUserService currentUser,
        GetTicketsQueryHandler getTickets,
        GetTicketByIdQueryHandler getTicketById,
        CreateTicketCommandHandler createTicket,
        AssignTicketCommandHandler assignTicket,
        ChangeTicketStatusCommandHandler changeStatus)
    {
        _currentUser = currentUser;
        _getTickets = getTickets;
        _getTicketById = getTicketById;
        _createTicket = createTicket;
        _assignTicket = assignTicket;
        _changeStatus = changeStatus;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketListItemDto>>> List(
        [FromQuery] TicketStatus? status,
        [FromQuery] Guid? categoryId,
        CancellationToken ct)
    {
        var query = new GetTicketsQuery(
            RequestingUserId: _currentUser.UserId,
            RequestingUserRole: _currentUser.Role,
            StatusFilter: status,
            CategoryFilter: categoryId);

        var result = await _getTickets.HandleAsync(query, ct);
        return Ok(result);
    }

    public sealed record CreateTicketRequest(
        string Title,
        string Description,
        TicketPriority Priority,
        Guid CategoryId);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketListItemDto>> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetTicketByIdQuery(
            TicketId: id,
            RequestingUserId: _currentUser.UserId,
            RequestingUserRole: _currentUser.Role);

        var result = await _getTicketById.HandleAsync(query, ct);
        return Ok(result);
    }
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateTicketRequest request,
        CancellationToken ct)
    {
        var command = new CreateTicketCommand(
            Title: request.Title,
            Description: request.Description,
            Priority: request.Priority,
            CategoryId: request.CategoryId,
            CreatedById: _currentUser.UserId);

        var id = await _createTicket.HandleAsync(command, ct);
        return CreatedAtAction(nameof(List), new { id }, id);
    }

    public sealed record AssignTicketRequest(Guid AssignToUserId);

    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> Assign(
        Guid id,
        [FromBody] AssignTicketRequest request,
        CancellationToken ct)
    {
        var command = new AssignTicketCommand(
            TicketId: id,
            AssignToUserId: request.AssignToUserId,
            RequestingUserId: _currentUser.UserId,
            RequestingUserRole: _currentUser.Role);

        await _assignTicket.HandleAsync(command, ct);
        return NoContent();
    }

    public sealed record ChangeStatusRequest(TicketStatus TargetStatus);

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeStatusRequest request,
        CancellationToken ct)
    {
        var command = new ChangeTicketStatusCommand(
            TicketId: id,
            TargetStatus: request.TargetStatus,
            RequestingUserId: _currentUser.UserId,
            RequestingUserRole: _currentUser.Role);

        await _changeStatus.HandleAsync(command, ct);
        return NoContent();
    }
}