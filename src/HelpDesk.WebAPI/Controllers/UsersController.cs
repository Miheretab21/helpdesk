using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Users.Dtos;
using HelpDesk.Application.Users.Queries.GetUsers;
using HelpDesk.Application.Users.Queries.GetUserById;
using HelpDesk.Application.Users.Commands.CreateUser;
using HelpDesk.Application.Users.Commands.UpdateUser;
using HelpDesk.Application.Users.Commands.DeleteUser;
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
    private readonly CreateUserCommandHandler _createUser;
    private readonly UpdateUserCommandHandler _updateUser;
    private readonly DeleteUserCommandHandler _deleteUser;
    private readonly ICurrentUserService _currentUser;

    public UsersController(
        GetUsersQueryHandler getUsers,
        GetUserByIdQueryHandler getUserById,
        CreateUserCommandHandler createUser,
        UpdateUserCommandHandler updateUser,
        DeleteUserCommandHandler deleteUser,
        ICurrentUserService currentUser)
    {
        _getUsers = getUsers;
        _getUserById = getUserById;
        _createUser = createUser;
        _updateUser = updateUser;
        _deleteUser = deleteUser;
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

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        // Only admins can create users
        if (_currentUser.Role != UserRole.Admin)
            return Forbid();

        var command = new CreateUserCommand(
            Email: request.Email,
            FullName: request.FullName,
            Password: request.Password,
            Role: request.Role);

        var userId = await _createUser.HandleAsync(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = userId }, userId);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        // Only admins can update users
        if (_currentUser.Role != UserRole.Admin)
            return Forbid();

        var command = new UpdateUserCommand(
            UserId: id,
            FullName: request.FullName,
            Role: request.Role,
            Password: request.Password);

        await _updateUser.HandleAsync(command, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        // Only admins can delete users
        if (_currentUser.Role != UserRole.Admin)
            return Forbid();

        var command = new DeleteUserCommand(UserId: id);
        await _deleteUser.HandleAsync(command, ct);
        return NoContent();
    }
}

public sealed record CreateUserRequest(
    string Email,
    string FullName,
    string Password,
    UserRole Role);

public sealed record UpdateUserRequest(
    string? FullName,
    UserRole? Role,
    string? Password);

