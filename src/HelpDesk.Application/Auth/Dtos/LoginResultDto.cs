namespace HelpDesk.Application.Auth.Dtos;

public sealed record LoginResultDto(
    string Token,
    Guid UserId,
    string Email,
    string FullName,
    string Role);