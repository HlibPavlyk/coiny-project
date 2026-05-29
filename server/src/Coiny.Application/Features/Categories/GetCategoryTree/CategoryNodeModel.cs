namespace Coiny.Application.Features.Categories.GetCategoryTree;

public record CategoryNodeModel(
    int Id,
    string Slug,
    string Name,
    byte Level,
    bool IsLeaf,
    string? SubcategoryKind,
    int LotCountActive,
    IReadOnlyList<CategoryNodeModel> Children);
