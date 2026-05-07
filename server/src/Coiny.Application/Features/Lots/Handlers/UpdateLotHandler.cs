using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Identity;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Lots.Handlers;

public class UpdateLotHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<UpdateLotRequest, Result>
{
    public async Task<Result> Handle(UpdateLotRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        Lot? lot = await db.Lots.FirstOrDefaultAsync(l => l.Id == request.Id, ct);
        if (lot is null)
            return Result.Failure(Error.NotFound("Lot.NotFound", "Lot not found."));

        if (lot.SellerId != userId)
            return Result.Failure(Error.Forbidden("Lot.NotOwner", "Only the seller can edit this lot."));

        if (lot.Status != LotStatus.Draft)
            return Result.Failure(Error.Conflict("Lot.NotDraft", "Only Draft lots can be edited."));

        Category? category = await db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct);
        if (category is null)
            return Result.Failure(Error.NotFound("Category.NotFound", $"Category {request.CategoryId} does not exist."));

        if (!category.IsLeaf)
            return Result.Failure(Error.Validation("Category.NotLeaf", "Lots can only be attached to leaf categories."));

        lot.Title = request.Title.Trim();
        lot.Description = request.Description.Trim();
        lot.CategoryId = request.CategoryId;
        lot.Condition = request.Condition;
        lot.StartingPriceUahKopiykas = request.StartingPriceUahKopiykas;
        lot.CurrentPriceUahKopiykas = request.StartingPriceUahKopiykas;
        lot.EndsAt = request.EndsAt;
        lot.Attributes = request.Attributes.GetRawText();
        lot.UpdatedAt = clock.UtcNow;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
