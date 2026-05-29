using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Abstractions.ExternalServices.Payments;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Payments.GetConnectStatus;

public class GetConnectStatusHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IStripeClient stripe)
    : IRequestHandler<GetConnectStatusRequest, Result<ConnectStatusResponse>>
{
    public async Task<Result<ConnectStatusResponse>> Handle(GetConnectStatusRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure<ConnectStatusResponse>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return Result.Failure<ConnectStatusResponse>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        if (user.StripeAccountId is null)
            return Result.Failure<ConnectStatusResponse>(
                Error.NotFound("Stripe.AccountMissing", "No Stripe Connect account — start onboarding first."));

        StripeAccountInfo info;
        try
        {
            info = await stripe.GetAccountAsync(user.StripeAccountId, ct);
        }
        catch (Exception ex)
        {
            return Result.Failure<ConnectStatusResponse>(
                Error.Internal("Stripe.AccountFetchFailed", ex.Message));
        }

        bool onboardedNow = info.DetailsSubmitted && info.ChargesEnabled;
        if (onboardedNow && !user.StripeOnboarded)
        {
            user.StripeOnboarded = true;
            await db.SaveChangesAsync(ct);
        }

        return Result.Success(new ConnectStatusResponse(user.StripeOnboarded, info.RequirementsCurrentlyDue));
    }
}
