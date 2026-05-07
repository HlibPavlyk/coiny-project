using Coiny.Application.Abstractions.Data;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Categories.Models;
using Coiny.Application.Features.Categories.Requests;
using Coiny.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Coiny.Application.Features.Categories.Handlers;

public class GetCategoryTreeHandler(IApplicationDbContext db, IMemoryCache cache)
    : IRequestHandler<GetCategoryTreeRequest, Result<CategoryTreeModel>>
{
    private const string _cacheKey = "categories:tree";
    private static readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(5);

    public async Task<Result<CategoryTreeModel>> Handle(GetCategoryTreeRequest request, CancellationToken ct)
    {
        if (cache.TryGetValue(_cacheKey, out CategoryTreeModel? cached) && cached is not null)
            return Result.Success(cached);

        List<Category> rows = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Level)
            .ThenBy(c => c.DisplaySort)
            .ToListAsync(ct);

        CategoryTreeModel tree = BuildTree(rows);

        cache.Set(_cacheKey, tree, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheTtl,
        });

        return Result.Success(tree);
    }

    private static CategoryTreeModel BuildTree(List<Category> rows)
    {
        var childrenByParent = rows
            .Where(c => c.ParentId is not null)
            .GroupBy(c => c.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(MapNode).ToList());

        var roots = rows
            .Where(c => c.ParentId is null)
            .Select(MapNode)
            .ToList();

        AttachChildren(roots, childrenByParent);

        return new CategoryTreeModel(roots);

        CategoryNodeModel MapNode(Category c) => new (
            c.Id,
            c.Slug,
            c.Name,
            c.Level,
            c.IsLeaf,
            c.SubcategoryKind?.ToString(),
            c.LotCountActive,
            []);
    }

    private static void AttachChildren(
        List<CategoryNodeModel> nodes,
        IReadOnlyDictionary<int, List<CategoryNodeModel>> childrenByParent)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            CategoryNodeModel node = nodes[i];
            if (childrenByParent.TryGetValue(node.Id, out List<CategoryNodeModel>? kids))
            {
                AttachChildren(kids, childrenByParent);
                nodes[i] = node with { Children = kids };
            }
        }
    }
}
