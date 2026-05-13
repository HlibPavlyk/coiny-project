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

/// <summary>
/// Public seller-scoped lot listing. Whitelists <c>Active</c> and <c>Sold</c> only — anything
/// else (Draft, EndedNoSale, Cancelled) is invisible to non-owners and surfaces as a validation
/// error to prevent privacy regressions if the controller is wired wrong.
/// </summary>
public class GetLotsBySellerHandler(IApplicationDbContext db)
    : IRequestHandler<GetLotsBySellerRequest, Result<Paginated<LotCardModel>>>
{
    public async Task<Result<Paginated<LotCardModel>>> Handle(GetLotsBySellerRequest request, CancellationToken ct)
    {
        if (request.Status is not (LotStatus.Active or LotStatus.Sold))
            return Result.Failure<Paginated<LotCardModel>>(
                Error.Validation("Lot.StatusNotPublic", "Only Active and Sold lots are visible on a public profile."));

        Result<IQueryable<Lot>> sortResult = db.Lots
            .AsNoTracking()
            .Where(l => l.SellerId == request.SellerId && l.Status == request.Status)
            .SortBy(
                request.SortBy,
                l => l.EndsAt,
                l => l.CreatedAt,
                l => l.CurrentPriceUahKopiykas,
                l => l.BidCount);

        if (sortResult.IsFailure)
            return Result.Failure<Paginated<LotCardModel>>(sortResult.Error);

        int totalCount = await sortResult.Value.CountAsync(ct);

        List<LotCardModel> items = await sortResult.Value
            .Paginate(request)
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
}
