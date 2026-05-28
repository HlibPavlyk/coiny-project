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

public class ConnectOnboardHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IStripeClient stripe)
    : IRequestHandler<ConnectOnboardRequest, Result<ConnectOnboardResponse>>
{
    public async Task<Result<ConnectOnboardResponse>> Handle(ConnectOnboardRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure<ConnectOnboardResponse>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return Result.Failure<ConnectOnboardResponse>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        if (string.IsNullOrWhiteSpace(user.Email))
            return Result.Failure<ConnectOnboardResponse>(
                Error.Validation("Stripe.NoEmail", "Stripe Connect requires an email on the user account."));

        // Block onboarding for unverified emails. A Stripe Connect account is a financial
        // record bound to this email; if an unverified address is later compromised, the
        // attacker could prove ownership and divert payouts. Verification belongs upstream.
        if (!user.EmailVerified)
            return Result.Failure<ConnectOnboardResponse>(
                Error.Validation("Stripe.EmailNotVerified", "Verify your email before connecting Stripe."));

        try
        {
            if (user.StripeAccountId is null)
            {
                StripeAccountInfo created = await stripe.CreateConnectAccountAsync(user.Email, ct);
                user.StripeAccountId = created.Id;
                await db.SaveChangesAsync(ct);
            }

            StripeAccountLink link = await stripe.CreateOnboardingLinkAsync(user.StripeAccountId, ct);
            return Result.Success(new ConnectOnboardResponse(link.Url, link.ExpiresAt));
        }
        catch (Exception ex)
        {
            return Result.Failure<ConnectOnboardResponse>(
                Error.Internal("Stripe.OnboardFailed", ex.Message));
        }
    }
}
