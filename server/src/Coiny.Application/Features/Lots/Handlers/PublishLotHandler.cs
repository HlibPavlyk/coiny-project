using System.Text.Json;
using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Abstractions.Jobs;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Lots.Handlers;

public class PublishLotHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IJobScheduler jobScheduler,
    IDateTimeProvider clock)
    : IRequestHandler<PublishLotRequest, Result<PublishedLotModel>>
{
    public async Task<Result<PublishedLotModel>> Handle(PublishLotRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure<PublishedLotModel>(Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        Lot? lot = await db.Lots
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == request.Id, ct);

        if (lot is null)
            return Result.Failure<PublishedLotModel>(Error.NotFound("Lot.NotFound", "Lot not found."));

        if (lot.SellerId != userId)
            return Result.Failure<PublishedLotModel>(Error.Forbidden("Lot.NotOwner", "Only the seller can publish this lot."));

        if (lot.Status != LotStatus.Draft)
            return Result.Failure<PublishedLotModel>(Error.Conflict("Lot.NotDraft", "Only Draft lots can be published."));

        if (lot.Images.Count == 0)
            return Result.Failure<PublishedLotModel>(Error.Validation("Lot.NoImages", "At least one image is required to publish."));

        DateTime now = clock.UtcNow;
        if (lot.EndsAt < now.AddHours(1) || lot.EndsAt > now.AddDays(7))
            return Result.Failure<PublishedLotModel>(Error.Validation(
                "Lot.EndsAtOutOfWindow",
                "EndsAt must be at least 1 hour and at most 7 days in the future."));

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        lot.Status = LotStatus.Active;
        lot.StartsAt = now;
        lot.UpdatedAt = now;

        using JsonDocument attrs = JsonDocument.Parse(lot.Attributes);
        LotPublishedPayload payload = new(
            lot.Id,
            lot.Title,
            lot.Description,
            lot.CategoryId,
            lot.Condition.ToString(),
            lot.CurrentPriceUahKopiykas,
            lot.BidCount,
            lot.EndsAt,
            lot.CreatedAt,
            attrs.RootElement.Clone());

        db.OutboxEvents.Add(new OutboxEvent
        {
            AggregateType = "Lot",
            AggregateId = lot.Id,
            EventType = LotPublishedPayload.EventType,
            Payload = payload.Serialize(),
            CreatedAt = now,
        });

        await db.Categories
            .Where(c => c.Id == lot.CategoryId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.LotCountActive, c => c.LotCountActive + 1), ct);

        lot.AuctionCloseJobId = jobScheduler.ScheduleAuctionClose(lot.Id, lot.EndsAt);

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Result.Success(new PublishedLotModel(lot.Id, lot.Status, lot.StartsAt, lot.EndsAt));
    }
}
