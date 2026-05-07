namespace Coiny.Application.Features.Categories.Models;

public record CategoryNodeModel(
    int Id,
    string Slug,
    string Name,
    byte Level,
    bool IsLeaf,
    string? SubcategoryKind,
    int LotCountActive,
    IReadOnlyList<CategoryNodeModel> Children);
