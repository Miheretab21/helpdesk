using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Common;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public UpdateUserCommandHandler(IUserRepository users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public async Task HandleAsync(UpdateUserCommand command, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(command.UserId, ct)
            ?? throw new DomainException("User not found.");

        // Update full name if provided
        if (!string.IsNullOrWhiteSpace(command.FullName))
        {
            var currentFullName = user.FullName;
            user.GetType().GetProperty("FullName")?.SetValue(user, command.FullName.Trim());
        }

        // Update role if provided
        if (command.Role.HasValue)
        {
            user.GetType().GetProperty("Role")?.SetValue(user, command.Role.Value);
        }

        // Update password if provided
        if (!string.IsNullOrWhiteSpace(command.Password))
        {
            var passwordHash = _hasher.Hash(command.Password);
            user.GetType().GetProperty("PasswordHash")?.SetValue(user, passwordHash);
        }

        await _users.UpdateAsync(user, ct);
    }
}
