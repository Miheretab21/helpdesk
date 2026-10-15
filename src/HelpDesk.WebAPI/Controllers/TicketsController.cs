using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Tickets.Commands.AssignTicket;
using HelpDesk.Application.Tickets.Commands.ChangeTicketStatus;
using HelpDesk.Application.Tickets.Commands.CreateTicket;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Application.Tickets.Queries.GetTicketById;
using HelpDesk.Application.Tickets.Queries.GetTickets;
using HelpDesk.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HelpDesk.Application.Tickets.Commands.CancelTicket;
using HelpDesk.Application.Tickets.Commands.EditTicket;

namespace HelpDesk.WebAPI.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public sealed class TicketsController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly GetTicketsQueryHandler _getTickets;
    private readonly GetTicketByIdQueryHandler _getTicketById;
    private readonly CreateTicketCommandHandler _createTicket;
    private readonly AssignTicketCommandHandler _assignTicket;
    private readonly ChangeTicketStatusCommandHandler _changeStatus;
    private readonly CancelTicketCommandHandler _cancelTicket;
    private readonly EditTicketCommandHandler _editTicket;
    public TicketsController(
        ICurrentUserService currentUser,
        GetTicketsQueryHandler getTickets,
        GetTicketByIdQueryHandler getTicketById,
        CreateTicketCommandHandler createTicket,
        AssignTicketCommandHandler assignTicket,
        ChangeTicketStatusCommandHandler changeStatus,
        CancelTicketCommandHandler cancelTicket,
        EditTicketCommandHandler editTicket)
    {
        _currentUser = currentUser;
        _getTickets = getTickets;
        _getTicketById = getTicketById;
        _createTicket = createTicket;
        _assignTicket = assignTicket;
        _changeStatus = changeStatus;
        _cancelTicket = cancelTicket;
        _editTicket = editTicket;
    }

    // -----------------------------------------------------------------------
    // Request / response contracts
    // -----------------------------------------------------------------------
    public sealed record CreateTicketRequest(
        string Title,
        string Description,
        TicketPriority Priority,
        Guid CategoryId);

    public sealed record CreateTicketResponse(Guid Id);

    public sealed record AssignTicketRequest(Guid AssignToUserId);

    public sealed record ChangeStatusRequest(TicketStatus TargetStatus);
    public sealed record EditTicketRequest(
    string Title,
    string Description,
    Guid CategoryId);

    // -----------------------------------------------------------------------
    // Endpoints
    // -----------------------------------------------------------------------
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketListItemDto>>> List(
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var query = new GetTicketsQuery(
            RequestingUserId: _currentUser.UserId,
            RequestingUserRole: _currentUser.Role,
            StatusFilter: status,
            PriorityFilter: priority,
            CategoryFilter: categoryId,
            SearchFilter: search);

        var result = await _getTickets.HandleAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetTicketByIdQuery(
            TicketId: id,
            RequestingUserId: _currentUser.UserId,
            RequestingUserRole: _currentUser.Role);

        var result = await _getTicketById.HandleAsync(query, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateTicketResponse>> Create(
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
        return CreatedAtAction(nameof(GetById), new { id }, new CreateTicketResponse(id));
    }

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
    [HttpPost("{id:guid}/cancel")]
public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
{
    var command = new CancelTicketCommand(
        TicketId: id,
        RequestingUserId: _currentUser.UserId);

    await _cancelTicket.HandleAsync(command, ct);
    return NoContent();
}
[HttpPut("{id:guid}")]
public async Task<IActionResult> Edit(
    Guid id,
    [FromBody] EditTicketRequest request,
    CancellationToken ct)
{
    var command = new EditTicketCommand(
        TicketId: id,
        Title: request.Title,
        Description: request.Description,
        CategoryId: request.CategoryId,
        RequestingUserId: _currentUser.UserId,
        RequestingUserRole: _currentUser.Role);

    await _editTicket.HandleAsync(command, ct);
    return NoContent();
}

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