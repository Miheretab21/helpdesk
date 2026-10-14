using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Auth.Commands.Login;
using HelpDesk.Application.Auth.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly LoginCommandHandler _login;
    private readonly ICurrentUserService _currentUser;

    public AuthController(LoginCommandHandler login, ICurrentUserService currentUser)
    {
        _login = login;
        _currentUser = currentUser;
    }

    public sealed record LoginRequest(string Email, string Password);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResultDto>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await _login.HandleAsync(
            new LoginCommand(request.Email, request.Password), ct);

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> Me()
    {
        return Ok(new
        {
            userId = _currentUser.UserId,
            role = _currentUser.Role.ToString(),
        });
    }
}