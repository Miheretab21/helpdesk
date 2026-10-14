using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Abstractions;

public interface IJwtTokenGenerator
{
    string Generate(User user);
}