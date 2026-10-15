using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Users.Dtos;

namespace HelpDesk.Application.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler
{
    private readonly IUserRepository _users;

    public GetUsersQueryHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<IReadOnlyList<UserListItemDto>> HandleAsync(
        GetUsersQuery query,
        CancellationToken ct = default)
    {
        var users = await _users.ListAsync(ct);

        return users
            .Select(u => new UserListItemDto(
                u.Id,
                u.FullName,
                u.Email,
                u.Role))
            .ToList();
    }
}
