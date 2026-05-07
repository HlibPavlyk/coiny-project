using Coiny.Application.Abstractions.Data;
using Coiny.Application.Common.Extensions;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Lots.Handlers;

public class GetLotsByCategoryHandler(IApplicationDbContext db)
    : IRequestHandler<GetLotsByCategoryRequest, Result<Paginated<LotCardModel>>>
{
    public async Task<Result<Paginated<LotCardModel>>> Handle(GetLotsByCategoryRequest request, CancellationToken ct)
    {
        List<Category> all = await db.Categories.AsNoTracking().ToListAsync(ct);

        Category? root = all.FirstOrDefault(c => c.Id == request.CategoryId);
        if (root is null)
            return Result.Failure<Paginated<LotCardModel>>(
                Error.NotFound("Category.NotFound", $"Category {request.CategoryId} does not exist."));

        var leafIds = CollectLeafIds(root, all).ToList();

        Result<IQueryable<Lot>> sortResult = db.Lots
            .AsNoTracking()
            .Where(l => l.Status == LotStatus.Active && leafIds.Contains(l.CategoryId))
            .SortBy(
                request.Paginate.SortBy,
                l => l.EndsAt,
                l => l.CreatedAt,
                l => l.CurrentPriceUahKopiykas,
                l => l.BidCount);

        if (sortResult.IsFailure)
            return Result.Failure<Paginated<LotCardModel>>(sortResult.Error);

        int totalCount = await sortResult.Value.CountAsync(ct);

        List<LotCardModel> items = await sortResult.Value
            .Paginate(request.Paginate)
            .Select(l => new LotCardModel(
                l.Id,
                l.Title,
                l.Images.OrderBy(i => i.DisplayOrder).Select(i => i.PublicUrl).FirstOrDefault() ?? string.Empty,
                l.CurrentPriceUahKopiykas,
                l.BidCount,
                l.EndsAt))
            .ToListAsync(ct);

        return Result.Success(new Paginated<LotCardModel>(totalCount, items));
    }

    private static IEnumerable<int> CollectLeafIds(Category root, IReadOnlyList<Category> all)
    {
        if (root.IsLeaf)
        {
            yield return root.Id;
            yield break;
        }

        foreach (Category child in all.Where(c => c.ParentId == root.Id))
        {
            foreach (int id in CollectLeafIds(child, all))
                yield return id;
        }
    }
}
