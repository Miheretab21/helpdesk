using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Common;

namespace HelpDesk.Application.Users.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler
{
    private readonly IUserRepository _users;

    public DeleteUserCommandHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task HandleAsync(DeleteUserCommand command, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(command.UserId, ct)
            ?? throw new DomainException("User not found.");

        await _users.DeleteAsync(user, ct);
    }
}
