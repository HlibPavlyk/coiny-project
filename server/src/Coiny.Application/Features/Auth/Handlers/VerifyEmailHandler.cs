using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Requests;
using Coiny.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Coiny.Application.Features.Auth.Services;

namespace Coiny.Application.Features.Auth.Handlers;

public class VerifyEmailHandler(IApplicationDbContext db, IDateTimeProvider clock)
    : IRequestHandler<VerifyEmailRequest, Result>
{
    public async Task<Result> Handle(VerifyEmailRequest request, CancellationToken ct)
    {
        string tokenHash = VerificationTokenFactory.Hash(request.Token);

        EmailVerificationToken? token = await db.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (token is null)
            return Result.Failure(Error.NotFound("Auth.TokenNotFound", "Verification token not found."));

        DateTime now = clock.UtcNow;

        if (token.UsedAt is not null)
            return Result.Failure(Error.Conflict("Auth.TokenAlreadyUsed", "Token already used."));

        if (token.ExpiresAt < now)
            return Result.Failure(Error.Conflict("Auth.TokenExpired", "Token expired."));

        User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == token.UserId, ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));

        token.UsedAt = now;
        user.EmailVerified = true;
        user.UpdatedAt = now;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
