using Coiny.Domain.Entities;

namespace Coiny.Application.Features.Categories;

/// <summary>
/// Resolves a category's full path by walking the parent chain to the root. Pure — operates on a
/// pre-loaded category lookup so callers control how categories are fetched/cached.
/// </summary>
public static class CategoryPathResolver
{
    /// <summary>Root-to-leaf category names, e.g. <c>["Coins", "Ukraine", "Hryvnia"]</c>.</summary>
    public static IReadOnlyList<string> NamesFromRoot(int leafId, IReadOnlyDictionary<int, Category> byId)
    {
        List<string> path = [];
        int? cursor = leafId;
        while (cursor is int id && byId.TryGetValue(id, out Category? node))
        {
            path.Add(node.Name);
            cursor = node.ParentId;
        }
        path.Reverse();
        return path;
    }
}
