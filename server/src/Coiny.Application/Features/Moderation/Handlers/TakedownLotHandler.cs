using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Authorization;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Moderation.Requests;
using Coiny.Application.Features.Lots.Events;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Moderation.Handlers;

/// <summary>
/// Admin-only soft-delete of a lot. The lot stays in Postgres (audit) but is hidden from public
/// listings and dropped from the Meilisearch index: a <c>LotDeleted</c> outbox event triggers the
/// sync, which removes the doc because the soft-deleted lot is filtered out of the reconcile read.
/// Role is enforced at the controller via <c>[Authorize(Roles = "Admin")]</c>; re-checked here.
/// </summary>
public class TakedownLotHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<TakedownLotRequest, Result>
{
    public async Task<Result> Handle(TakedownLotRequest request, CancellationToken ct)
    {
        if (!currentUser.CanModerate())
            return Result.Failure(Error.Forbidden("Admin.Forbidden", "Moderator or Administrator role required."));

        // Ignore the soft-delete query filter so an already-deleted lot is a 409, not a 404.
        Lot? lot = await db.Lots
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Id == request.LotId, ct);

        if (lot is null)
            return Result.Failure(Error.NotFound("Lot.NotFound", "Lot not found."));

        if (lot.IsDeleted)
            return Result.Failure(Error.Conflict("Lot.AlreadyDeleted", "This lot is already deleted."));

        DateTime now = clock.UtcNow;
        bool wasActive = lot.Status == LotStatus.Active;

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        lot.IsDeleted = true;
        lot.DeletedAt = now;
        lot.DeletedByUserId = currentUser.UserId;
        lot.UpdatedAt = now;

        if (wasActive)
        {
            Category? category = await db.Categories.FirstOrDefaultAsync(c => c.Id == lot.CategoryId, ct);
            if (category is not null)
                category.LotCountActive = Math.Max(category.LotCountActive - 1, 0);
        }

        db.SearchOutboxEvents.Add(new SearchOutboxEvent
        {
            AggregateType = "Lot",
            AggregateId = lot.Id,
            EventType = LotDeletedPayload.EventType,
            Payload = new LotDeletedPayload(lot.Id).Serialize(),
            CreatedAt = now,
        });

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Result.Success();
    }
}
