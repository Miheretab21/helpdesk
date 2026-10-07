using HelpDesk.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.WebAPI.Middleware;

public sealed class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DomainExceptionMiddleware> _logger;

    public DomainExceptionMiddleware(RequestDelegate next, ILogger<DomainExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (NotFoundException ex)
    {
        _logger.LogInformation("Resource not found: {Message}", ex.Message);
        await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Not found", ex.Message);
    }
    catch (DomainException ex)
    {
        _logger.LogWarning(ex, "Domain rule violated: {Message}", ex.Message);
        await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Business rule violation", ex.Message);
    }
}

private static async Task WriteProblemAsync(HttpContext context, int status, string title, string detail)
{
    context.Response.StatusCode = status;
    context.Response.ContentType = "application/problem+json";

    var problem = new ProblemDetails
    {
        Status = status,
        Title = title,
        Detail = detail,
        Type = "https://tools.ietf.org/html/rfc7231"
    };

    await context.Response.WriteAsJsonAsync(problem);
}
}