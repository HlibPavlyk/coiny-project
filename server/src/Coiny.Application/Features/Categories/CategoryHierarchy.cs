using Coiny.Domain.Entities;

namespace Coiny.Application.Features.Categories;

/// <summary>
/// In-memory navigation over the category tree. Pure — operates on a pre-loaded category set so
/// callers stay in control of how categories are fetched and cached.
/// </summary>
public static class CategoryHierarchy
{
    /// <summary>Root-to-leaf category names, e.g. <c>["Coins", "Ukraine", "Hryvnia"]</c>.</summary>
    public static IReadOnlyList<string> NamesFromRoot(int leafId, IReadOnlyDictionary<int, Category> byId)
    {
        List<string> path = [];
        int? cursor = leafId;
        while (cursor is { } id && byId.TryGetValue(id, out Category? node))
        {
            path.Add(node.Name);
            cursor = node.ParentId;
        }
        path.Reverse();
        return path;
    }

    /// <summary>Ids of every leaf category at or below <paramref name="root"/>.</summary>
    public static IReadOnlyList<int> LeafIdsUnder(Category root, IReadOnlyList<Category> all)
    {
        List<int> leafIds = [];
        Collect(root);
        return leafIds;

        void Collect(Category node)
        {
            if (node.IsLeaf)
            {
                leafIds.Add(node.Id);
                return;
            }

            foreach (Category child in all.Where(c => c.ParentId == node.Id))
                Collect(child);
        }
    }
}
