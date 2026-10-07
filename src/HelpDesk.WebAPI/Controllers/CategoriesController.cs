using HelpDesk.Application.Categories.Dtos;
using HelpDesk.Application.Categories.Queries.GetCategories;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.WebAPI.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly GetCategoriesQueryHandler _getCategories;

    public CategoriesController(GetCategoriesQueryHandler getCategories)
    {
        _getCategories = getCategories;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> List(CancellationToken ct)
    {
        var result = await _getCategories.HandleAsync(new GetCategoriesQuery(), ct);
        return Ok(result);
    }
}