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
using HelpDesk.Domain.Entities;

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
    private readonly ITicketRepository _tickets;
    private readonly ITicketAttachmentRepository _attachments;
    private readonly ITicketAuditRepository _audit;
    private readonly IUserRepository _users;
    private readonly IWebHostEnvironment _environment;
    public TicketsController(
        ICurrentUserService currentUser,
        GetTicketsQueryHandler getTickets,
        GetTicketByIdQueryHandler getTicketById,
        CreateTicketCommandHandler createTicket,
        AssignTicketCommandHandler assignTicket,
        ChangeTicketStatusCommandHandler changeStatus,
        CancelTicketCommandHandler cancelTicket,
        EditTicketCommandHandler editTicket,
        ITicketRepository tickets,
        ITicketAttachmentRepository attachments,
        ITicketAuditRepository audit,
        IUserRepository users,
        IWebHostEnvironment environment)
    {
        _currentUser = currentUser;
        _getTickets = getTickets;
        _getTicketById = getTicketById;
        _createTicket = createTicket;
        _assignTicket = assignTicket;
        _changeStatus = changeStatus;
        _cancelTicket = cancelTicket;
        _editTicket = editTicket;
        _tickets = tickets;
        _attachments = attachments;
        _audit = audit;
        _users = users;
        _environment = environment;
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
    public sealed record BulkActionRequest(IReadOnlyList<Guid> TicketIds, string Action, Guid? AssignToUserId = null);
    public sealed record BulkActionResponse(int ProcessedCount, IReadOnlyList<Guid> SkippedTicketIds);
    public sealed record AttachmentDto(Guid Id, string FileName, string ContentType, long SizeBytes, Guid UploadedById, DateTime CreatedAt);
    public sealed record AuditDto(Guid Id, string Action, string? Details, Guid? ActorId, DateTime OccurredAt);

    // -----------------------------------------------------------------------
    // Endpoints
    // -----------------------------------------------------------------------
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketListItemDto>>> List(
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search,
        [FromQuery] Guid? assignedToId,
        [FromQuery] bool? unassignedOnly,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] TicketStatus? excludeStatus,
        [FromQuery] string? excludeStatuses,
        CancellationToken ct)
    {
        var query = new GetTicketsQuery(
            RequestingUserId: _currentUser.UserId,
            RequestingUserRole: _currentUser.Role,
            StatusFilter: status,
            PriorityFilter: priority,
            CategoryFilter: categoryId,
            SearchFilter: search,
            ExcludeStatusFilter: excludeStatus,
            ExcludeStatusesFilter: excludeStatuses,
            AssignedToIdFilter: assignedToId,
            UnassignedOnly: unassignedOnly,
            SortBy: sortBy,
            SortDirection: sortDirection);

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
        await AddAuditAsync(id, "Created", "Ticket created", ct);
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
        var assignee = await _users.GetByIdAsync(request.AssignToUserId, ct);
        await AddAuditAsync(id, "Assigned", $"Assigned to {assignee?.FullName ?? request.AssignToUserId.ToString()}", ct);
        return NoContent();
    }
    [HttpPost("{id:guid}/cancel")]
public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
{
    var command = new CancelTicketCommand(
        TicketId: id,
        RequestingUserId: _currentUser.UserId);

    await _cancelTicket.HandleAsync(command, ct);
    await AddAuditAsync(id, "Cancelled", "Ticket cancelled", ct);
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
    await AddAuditAsync(id, "Edited", "Ticket details updated", ct);
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
        await AddAuditAsync(id, "Status changed", $"Status changed to {request.TargetStatus}", ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reopen")]
    public async Task<IActionResult> Reopen(Guid id, CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(id, ct) ?? throw new HelpDesk.Domain.Common.NotFoundException("Ticket not found.");
        ticket.Reopen(_currentUser.UserId, _currentUser.Role == UserRole.Admin);
        await _tickets.UpdateAsync(ticket, ct);
        await AddAuditAsync(id, "Reopened", "Ticket reopened", ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/escalate")]
    public async Task<IActionResult> Escalate(Guid id, CancellationToken ct)
    {
        if (_currentUser.Role is not (UserRole.Agent or UserRole.Admin)) return Forbid();
        var ticket = await _tickets.GetByIdAsync(id, ct) ?? throw new HelpDesk.Domain.Common.NotFoundException("Ticket not found.");
        if (!CanAccess(ticket)) return NotFound();
        ticket.Escalate(); await _tickets.UpdateAsync(ticket, ct);
        await AddAuditAsync(id, "Escalated", "SLA escalation raised", ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/attachments")]
    public async Task<ActionResult<IReadOnlyList<AttachmentDto>>> ListAttachments(Guid id, CancellationToken ct)
    {
        await EnsureAccessAsync(id, ct);
        var items = await _attachments.ListByTicketAsync(id, ct);
        return Ok(items.Select(x => new AttachmentDto(x.Id, x.OriginalFileName, x.ContentType, x.SizeBytes, x.UploadedById, x.CreatedAt)));
    }

    [HttpPost("{id:guid}/attachments")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<AttachmentDto>> UploadAttachment(Guid id, IFormFile file, CancellationToken ct)
    {
        await EnsureAccessAsync(id, ct);
        if (file is null || file.Length == 0 || file.Length > 10 * 1024 * 1024) return BadRequest("Choose a file up to 10 MB.");
        var safeName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(safeName).ToLowerInvariant();
        var allowed = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".txt", ".log", ".doc", ".docx", ".xlsx", ".csv" };
        if (!allowed.Contains(extension)) return BadRequest("This file type is not allowed.");
        var directory = Path.Combine(_environment.ContentRootPath, "App_Data", "attachments");
        Directory.CreateDirectory(directory);
        var storedName = $"{Guid.NewGuid():N}{extension}";
        await using (var stream = System.IO.File.Create(Path.Combine(directory, storedName))) await file.CopyToAsync(stream, ct);
        var attachment = new TicketAttachment(id, _currentUser.UserId, safeName, storedName, file.ContentType, file.Length);
        await _attachments.AddAsync(attachment, ct);
        await AddAuditAsync(id, "Attachment added", safeName, ct);
        return Ok(new AttachmentDto(attachment.Id, attachment.OriginalFileName, attachment.ContentType, attachment.SizeBytes, attachment.UploadedById, attachment.CreatedAt));
    }

    [HttpGet("{id:guid}/attachments/{attachmentId:guid}/download")]
    public async Task<IActionResult> DownloadAttachment(Guid id, Guid attachmentId, CancellationToken ct)
    {
        await EnsureAccessAsync(id, ct);
        var attachment = await _attachments.GetByIdAsync(attachmentId, ct);
        if (attachment is null || attachment.TicketId != id) return NotFound();
        var path = Path.Combine(_environment.ContentRootPath, "App_Data", "attachments", attachment.StoredFileName);
        if (!System.IO.File.Exists(path)) return NotFound();
        return File(System.IO.File.OpenRead(path), attachment.ContentType, attachment.OriginalFileName);
    }

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<AuditDto>>> History(Guid id, CancellationToken ct)
    {
        await EnsureAccessAsync(id, ct);
        var entries = await _audit.ListByTicketAsync(id, ct);
        // Older audit rows embedded the raw assignee id (e.g. "Assigned to <guid>"); resolve
        // any user id in the text to a display name so history stays human-readable.
        var namesById = (await _users.ListAsync(ct)).ToDictionary(x => x.Id, x => x.FullName);
        return Ok(entries.Select(x => new AuditDto(x.Id, x.Action, ResolveUserNames(x.Details, namesById), x.ActorId, x.OccurredAt)));
    }

    private static string? ResolveUserNames(string? details, IReadOnlyDictionary<Guid, string> namesById)
    {
        if (string.IsNullOrEmpty(details)) return details;
        foreach (var (userId, fullName) in namesById)
            details = details.Replace(userId.ToString(), fullName, StringComparison.OrdinalIgnoreCase);
        return details;
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<BulkActionResponse>> Bulk([FromBody] BulkActionRequest request, CancellationToken ct)
    {
        if (_currentUser.Role is not (UserRole.Agent or UserRole.Admin)) return Forbid();
        var ids = request.TicketIds.Distinct().Take(100).ToArray();
        if (ids.Length == 0) return BadRequest("Select at least one ticket.");
        var processed = 0;
        var skipped = new List<Guid>();
        foreach (var id in ids)
        {
            var ticket = await _tickets.GetByIdAsync(id, ct);
            if (ticket is null || !CanAccess(ticket)) { skipped.Add(id); continue; }
            try
            {
                switch (request.Action.Trim().ToLowerInvariant())
                {
                    case "escalate": ticket.Escalate(); break;
                    case "assign":
                        if (_currentUser.Role != UserRole.Admin || request.AssignToUserId is null) return BadRequest("An admin must choose an assignee.");
                        ticket.AssignTo(request.AssignToUserId.Value); break;
                    case "start": ticket.StartWork(_currentUser.UserId, _currentUser.Role == UserRole.Admin); break;
                    case "resolve": ticket.Resolve(_currentUser.UserId, _currentUser.Role == UserRole.Admin); break;
                    default: return BadRequest("Unsupported bulk action.");
                }
                await _tickets.UpdateAsync(ticket, ct);
                await AddAuditAsync(id, $"Bulk {request.Action}", "Updated through bulk action", ct);
                processed++;
            }
            catch (HelpDesk.Domain.Common.DomainException) { skipped.Add(id); }
        }
        return Ok(new BulkActionResponse(processed, skipped));
    }

    private bool CanAccess(Ticket ticket) => _currentUser.Role switch
    {
        UserRole.Admin => true,
        UserRole.Agent => ticket.AssignedToId is null || ticket.AssignedToId == _currentUser.UserId,
        UserRole.Employee => ticket.CreatedById == _currentUser.UserId,
        _ => false
    };
    private async Task EnsureAccessAsync(Guid id, CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(id, ct) ?? throw new HelpDesk.Domain.Common.NotFoundException("Ticket not found.");
        if (!CanAccess(ticket)) throw new HelpDesk.Domain.Common.NotFoundException("Ticket not found.");
    }
    private Task AddAuditAsync(Guid ticketId, string action, string? details, CancellationToken ct) => _audit.AddAsync(new TicketAuditEntry(ticketId, _currentUser.UserId, action, details), ct);
}
