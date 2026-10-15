using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Common;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public CreateUserCommandHandler(IUserRepository users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public async Task<Guid> HandleAsync(CreateUserCommand command, CancellationToken ct = default)
    {
        // Check if user already exists
        var existingUser = await _users.GetByEmailAsync(command.Email, ct);
        if (existingUser != null)
            throw new DomainException($"User with email '{command.Email}' already exists.");

        // Hash password
        var passwordHash = _hasher.Hash(command.Password);

        // Create user
        var user = new User(command.Email, command.FullName, passwordHash, command.Role);

        await _users.AddAsync(user, ct);

        return user.Id;
    }
}
