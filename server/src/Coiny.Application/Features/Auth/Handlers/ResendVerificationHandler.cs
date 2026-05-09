using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Abstractions.Identity;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Requests;
using Coiny.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Auth.Handlers;

public class ResendVerificationHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<ResendVerificationRequest, Result>
{
    private static readonly TimeSpan ThrottleWindow = TimeSpan.FromMinutes(1);

    public async Task<Result> Handle(ResendVerificationRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not Guid userId)
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));

        if (user.EmailVerified)
            return Result.Failure(Error.Conflict("Auth.AlreadyVerified", "Email is already verified."));

        DateTime now = clock.UtcNow;
        DateTime throttleCutoff = now - ThrottleWindow;

        bool recentTokenExists = await db.EmailVerificationTokens
            .AnyAsync(t => t.UserId == userId && t.CreatedAt > throttleCutoff, ct);

        if (recentTokenExists)
            return Result.Failure(Error.RateLimited(
                "Auth.ResendThrottled",
                "Please wait 1 minute before requesting another verification email."));

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        await db.EmailVerificationTokens
            .Where(t => t.UserId == userId && t.UsedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAt, now), ct);

        string rawToken = VerificationTokenFactory.NewRawToken();
        string tokenHash = VerificationTokenFactory.Hash(rawToken);

        db.EmailVerificationTokens.Add(new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = now.AddHours(24),
            CreatedAt = now,
        });

        EmailVerificationPayload payload = new(userId, user.Email!, rawToken, tokenHash);
        db.EmailOutboxEvents.Add(new EmailOutboxEvent
        {
            AggregateType = "User",
            AggregateId = userId,
            EventType = EmailVerificationPayload.EventType,
            Payload = payload.Serialize(),
            CreatedAt = now,
        });

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Result.Success();
    }
}
