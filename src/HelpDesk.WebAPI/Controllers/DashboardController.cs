using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Dashboard.Dtos;
using HelpDesk.Application.Dashboard.Queries.GetMetrics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.WebAPI.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly GetMetricsQueryHandler _getMetrics;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(
        GetMetricsQueryHandler getMetrics,
        ICurrentUserService currentUser)
    {
        _getMetrics = getMetrics;
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
}
