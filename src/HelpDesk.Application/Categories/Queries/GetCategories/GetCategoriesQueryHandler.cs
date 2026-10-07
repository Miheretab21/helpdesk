using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Categories.Dtos;

namespace HelpDesk.Application.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler
{
    private readonly ICategoryRepository _categories;

    public GetCategoriesQueryHandler(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public async Task<IReadOnlyList<CategoryDto>> HandleAsync(
        GetCategoriesQuery query,
        CancellationToken ct = default)
    {
        var all = await _categories.ListActiveAsync(ct);
        return all.Select(c => new CategoryDto(c.Id, c.Name)).ToList();
    }
}