using System.ComponentModel.DataAnnotations;
using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Auth.Commands.Login;
using HelpDesk.Application.Auth.Dtos;
using HelpDesk.Domain.Common;
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

    public sealed class LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string Email { get; init; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(128, MinimumLength = 1, ErrorMessage = "Password must be between 1 and 128 characters.")]
        public string Password { get; init; } = string.Empty;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResultDto>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await _login.HandleAsync(
                new LoginCommand(request.Email.Trim(), request.Password), ct);

            return Ok(result);
        }
        catch (DomainException)
        {
            // Every failed attempt returns the exact same payload so the endpoint
            // cannot be used to discover which email addresses have accounts.
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication failed",
                detail: "Invalid email or password.");
        }
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