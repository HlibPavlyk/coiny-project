using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Lots.Handlers;

public class CreateLotHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<CreateLotRequest, Result<LotCreatedModel>>
{
    public async Task<Result<LotCreatedModel>> Handle(CreateLotRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure<LotCreatedModel>(Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        User? seller = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (seller is null)
            return Result.Failure<LotCreatedModel>(Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        if (!seller.EmailVerified)
            return Result.Failure<LotCreatedModel>(Error.Forbidden("Lot.EmailNotVerified", "Verify your email before creating lots."));

        if (!seller.StripeOnboarded)
            return Result.Failure<LotCreatedModel>(Error.Forbidden("Lot.StripeNotOnboarded", "Complete Stripe Connect onboarding before creating lots."));

        Category? category = await db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct);
        if (category is null)
            return Result.Failure<LotCreatedModel>(Error.NotFound("Category.NotFound", $"Category {request.CategoryId} does not exist."));

        if (!category.IsLeaf)
            return Result.Failure<LotCreatedModel>(Error.Validation("Category.NotLeaf", "Lots can only be attached to leaf categories."));

        DateTime now = clock.UtcNow;

        var lot = new Lot
        {
            Id = Guid.NewGuid(),
            SellerId = userId,
            CategoryId = request.CategoryId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Condition = request.Condition,
            StartingPriceUahKopiykas = request.StartingPriceUahKopiykas,
            CurrentPriceUahKopiykas = request.StartingPriceUahKopiykas,
            BidCount = 0,
            ViewCount = 0,
            Status = LotStatus.Draft,
            StartsAt = default,
            EndsAt = request.EndsAt,
            Attributes = request.Attributes.GetRawText(),
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Lots.Add(lot);
        await db.SaveChangesAsync(ct);

        return Result.Success(new LotCreatedModel(lot.Id));
    }
}
