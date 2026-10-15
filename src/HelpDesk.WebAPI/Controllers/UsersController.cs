using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Users.Dtos;
using HelpDesk.Application.Users.Queries.GetUsers;
using HelpDesk.Application.Users.Queries.GetUserById;
using HelpDesk.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.WebAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly GetUsersQueryHandler _getUsers;
    private readonly GetUserByIdQueryHandler _getUserById;
    private readonly ICurrentUserService _currentUser;

    public UsersController(
        GetUsersQueryHandler getUsers,
        GetUserByIdQueryHandler getUserById,
        ICurrentUserService currentUser)
    {
        _getUsers = getUsers;
        _getUserById = getUserById;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserListItemDto>>> List(CancellationToken ct)
    {
        // Only admins can list all users
        if (_currentUser.Role != UserRole.Admin)
            return Forbid();

        var query = new GetUsersQuery();
        var result = await _getUsers.HandleAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetUserByIdQuery(UserId: id);
        var result = await _getUserById.HandleAsync(query, ct);
        return Ok(result);
    }
}
