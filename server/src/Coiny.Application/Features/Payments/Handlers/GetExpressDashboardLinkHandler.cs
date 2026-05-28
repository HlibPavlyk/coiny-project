using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Abstractions.ExternalServices.Payments;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Payments.Models;
using Coiny.Application.Features.Payments.Requests;
using Coiny.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Payments.Handlers;

public class GetExpressDashboardLinkHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IStripeClient stripe)
    : IRequestHandler<GetExpressDashboardLinkRequest, Result<ExpressDashboardLinkResponse>>
{
    public async Task<Result<ExpressDashboardLinkResponse>> Handle(
        GetExpressDashboardLinkRequest request,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure<ExpressDashboardLinkResponse>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return Result.Failure<ExpressDashboardLinkResponse>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        if (user.StripeAccountId is null)
            return Result.Failure<ExpressDashboardLinkResponse>(
                Error.NotFound("Stripe.AccountMissing", "No Stripe Connect account — start onboarding first."));

        // Stripe rejects login_link requests on accounts that have not completed onboarding
        // (details_submitted=false). Surface a clean error rather than letting the SDK throw.
        if (!user.StripeOnboarded)
            return Result.Failure<ExpressDashboardLinkResponse>(
                Error.Validation("Stripe.NotOnboarded", "Finish Stripe onboarding before opening the dashboard."));

        try
        {
            string url = await stripe.CreateExpressDashboardLinkAsync(user.StripeAccountId, ct);
            return Result.Success(new ExpressDashboardLinkResponse(url));
        }
        catch (Exception ex)
        {
            return Result.Failure<ExpressDashboardLinkResponse>(
                Error.Internal("Stripe.DashboardLinkFailed", ex.Message));
        }
    }
}
