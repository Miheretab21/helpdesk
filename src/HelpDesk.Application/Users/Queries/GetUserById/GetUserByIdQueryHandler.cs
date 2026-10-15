using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Users.Dtos;
using HelpDesk.Domain.Common;

namespace HelpDesk.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler
{
    private readonly IUserRepository _users;

    public GetUserByIdQueryHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<UserDetailDto> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(query.UserId, ct);
        
        if (user is null)
            throw new NotFoundException($"User with ID {query.UserId} not found.");

        return new UserDetailDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.CreatedAt);
    }
}
