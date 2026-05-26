using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Identity;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Models;
using Coiny.Application.Features.Auth.Requests;
using Coiny.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Coiny.Application.Features.Auth.Events;
using Coiny.Application.Features.Auth.Services;

namespace Coiny.Application.Features.Auth.Handlers;

public class RegisterHandler(
    IIdentityService identityService,
    IJwtTokenGenerator tokenGenerator,
    IApplicationDbContext db,
    IDateTimeProvider clock)
    : IRequestHandler<RegisterRequest, Result<LoginSuccessModel>>
{
    private const string _defaultRole = "User";

    public async Task<Result<LoginSuccessModel>> Handle(RegisterRequest request, CancellationToken ct)
    {
        string normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == normalizedEmail, ct))
            return Result.Failure<LoginSuccessModel>(Error.Conflict("User.EmailInUse", "Email already in use."));

        DateTime now = clock.UtcNow;

        var user = new User
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailVerified = false,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? normalizedEmail.Split('@')[0]
                : request.DisplayName.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        Result<User> createResult = await identityService.CreateAsync(user, request.Password, ct);
        if (createResult.IsFailure)
            return Result.Failure<LoginSuccessModel>(createResult.Error);

        Result assignResult = await identityService.AssignRoleAsync(user, _defaultRole, ct);
        if (assignResult.IsFailure)
            return Result.Failure<LoginSuccessModel>(assignResult.Error);

        string rawToken = VerificationTokenFactory.NewRawToken();
        string tokenHash = VerificationTokenFactory.Hash(rawToken);

        db.EmailVerificationTokens.Add(new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = now.AddHours(24),
            CreatedAt = now,
        });

        EmailVerificationPayload payload = new(user.Id, user.Email!, rawToken, tokenHash);
        db.EmailOutboxEvents.Add(new EmailOutboxEvent
        {
            AggregateType = "User",
            AggregateId = user.Id,
            EventType = EmailVerificationPayload.EventType,
            Payload = payload.Serialize(),
            CreatedAt = now,
        });

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        IList<string> roles = await identityService.GetRolesAsync(user);
        AccessToken accessToken = tokenGenerator.IssueToken(user, roles);

        MeModel me = new(
            user.Id,
            user.Email!,
            user.EmailVerified,
            user.DisplayName,
            user.TrustScore,
            user.IsBanned,
            user.StripeOnboarded,
            [.. roles]);

        return Result.Success(new LoginSuccessModel(accessToken, me));
    }
}
