using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Dashboard.Dtos;
using HelpDesk.Application.Dashboard.Queries.GetMetrics;
using HelpDesk.Application.Dashboard.Queries.GetEmployeeDashboard;
using HelpDesk.Application.Dashboard.Queries.GetAgentDashboard;
using HelpDesk.Application.Dashboard.Queries.GetAdminDashboard;
using HelpDesk.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.WebAPI.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly GetMetricsQueryHandler _getMetrics;
    private readonly GetEmployeeDashboardQueryHandler _getEmployeeDashboard;
    private readonly GetAgentDashboardQueryHandler _getAgentDashboard;
    private readonly GetAdminDashboardQueryHandler _getAdminDashboard;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(
        GetMetricsQueryHandler getMetrics,
        GetEmployeeDashboardQueryHandler getEmployeeDashboard,
        GetAgentDashboardQueryHandler getAgentDashboard,
        GetAdminDashboardQueryHandler getAdminDashboard,
        ICurrentUserService currentUser)
    {
        _getMetrics = getMetrics;
        _getEmployeeDashboard = getEmployeeDashboard;
        _getAgentDashboard = getAgentDashboard;
        _getAdminDashboard = getAdminDashboard;
        _currentUser = currentUser;
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<DashboardMetricsDto>> GetMetrics(CancellationToken ct)
    {
        var query = new GetMetricsQuery(
            RequestingUserId: _currentUser.UserId,
            RequestingUserRole: _currentUser.Role);

        var result = await _getMetrics.HandleAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("my-dashboard")]
    public async Task<ActionResult> GetMyDashboard(CancellationToken ct)
    {
        return _currentUser.Role switch
        {
            UserRole.Employee => await GetEmployeeDashboardInternal(ct),
            UserRole.Agent => await GetAgentDashboardInternal(ct),
            UserRole.Admin => await GetAdminDashboardInternal(ct),
            _ => BadRequest("Unknown role")
        };
    }

    [HttpGet("employee")]
    public async Task<ActionResult<EmployeeDashboardDto>> GetEmployeeDashboard(CancellationToken ct)
    {
        return await GetEmployeeDashboardInternal(ct);
    }

    [HttpGet("agent")]
    public async Task<ActionResult<AgentDashboardDto>> GetAgentDashboard(CancellationToken ct)
    {
        return await GetAgentDashboardInternal(ct);
    }

    [HttpGet("admin")]
    public async Task<ActionResult<AdminDashboardDto>> GetAdminDashboard(CancellationToken ct)
    {
        if (_currentUser.Role != UserRole.Admin)
            return Forbid();

        return await GetAdminDashboardInternal(ct);
    }

    private async Task<ActionResult> GetEmployeeDashboardInternal(CancellationToken ct)
    {
        var query = new GetEmployeeDashboardQuery(
            RequestingUserId: _currentUser.UserId,
            RequestingUserRole: _currentUser.Role);

        var result = await _getEmployeeDashboard.HandleAsync(query, ct);
        return Ok(result);
    }

    private async Task<ActionResult> GetAgentDashboardInternal(CancellationToken ct)
    {
        var query = new GetAgentDashboardQuery(
            RequestingUserId: _currentUser.UserId,
            RequestingUserRole: _currentUser.Role);

        var result = await _getAgentDashboard.HandleAsync(query, ct);
        return Ok(result);
    }

    private async Task<ActionResult> GetAdminDashboardInternal(CancellationToken ct)
    {
        var query = new GetAdminDashboardQuery(
            RequestingUserId: _currentUser.UserId,
            RequestingUserRole: _currentUser.Role);

        var result = await _getAdminDashboard.HandleAsync(query, ct);
        return Ok(result);
    }
}
