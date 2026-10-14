using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Auth.Dtos;
using HelpDesk.Domain.Common;

namespace HelpDesk.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _tokens;

    public LoginCommandHandler(
        IUserRepository users,
        IPasswordHasher hasher,
        IJwtTokenGenerator tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<LoginResultDto> HandleAsync(LoginCommand command, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(command.Email, ct)
            ?? throw new DomainException("Invalid email or password.");

        if (!_hasher.Verify(command.Password, user.PasswordHash))
            throw new DomainException("Invalid email or password.");

        var token = _tokens.Generate(user);

        return new LoginResultDto(
            Token: token,
            UserId: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role.ToString());
    }
}