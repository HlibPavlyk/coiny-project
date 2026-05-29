using Coiny.Application.Abstractions.Infrastructure.Identity;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Register;
using Coiny.Application.Features.Auth.Shared;
using Coiny.Domain.Entities;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Coiny.Application.Tests.Features.Auth;

/// <summary>
/// Showcase for the registration flow's single-transaction guarantee. The handler bundles three
/// state changes — the Identity user row, the EmailVerificationToken row, and the EmailOutboxEvent
/// row — into one EF transaction so that an email-sending failure (handled later by the flush job)
/// never leaves the account inconsistent. The outbox row also guarantees that the verification
/// email is sent at-least-once even if the sync send-attempt would have failed.
/// </summary>
[Trait("Showcase", "true")]
public class RegisterHandlerTests
{
    private static readonly DateTime Now = new(2026, 5, 28, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Happy_path_creates_user_token_and_outbox_event_in_one_unit_of_work()
    {
        using var ctx = NewDb();
        var identity = new FakeIdentityService();
        var jwt = new FakeJwtTokenGenerator();
        var handler = new RegisterHandler(identity, jwt, ctx, new FakeClock(Now));

        Result<LoginSuccessModel> result = await handler.Handle(
            new RegisterRequest("Test@Example.com", "Secret123!", "Alice"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Me.Email.Should().Be("test@example.com");
        result.Value.Me.DisplayName.Should().Be("Alice");
        result.Value.Token.Token.Should().NotBeNullOrEmpty();

        // User row was inserted by the (fake) Identity service.
        identity.CreatedUsers.Should().ContainSingle(u => u.Email == "test@example.com");
        identity.AssignedRoles.Should().ContainSingle(r => r.Role == "User");

        // Verification token row was added in the same transaction.
        EmailVerificationToken token = await ctx.EmailVerificationTokens.SingleAsync();
        token.TokenHash.Should().NotBeNullOrEmpty();
        token.ExpiresAt.Should().Be(Now.AddHours(24));

        // Outbox event row was added in the same transaction (carries the raw token so the
        // flush job can render the verification URL — not stored on the token row itself).
        EmailOutboxEvent outbox = await ctx.EmailOutboxEvents.SingleAsync();
        outbox.AggregateType.Should().Be(nameof(User));
        outbox.EventType.Should().Be(EmailVerificationPayload.EventType);
        outbox.ProcessedAt.Should().BeNull();
        outbox.Payload.Should().Contain("\"toAddress\":\"test@example.com\"");
    }

    [Fact]
    public async Task Email_already_in_use_returns_conflict_and_writes_nothing()
    {
        using var ctx = NewDb();
        ctx.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "taken@example.com",
            UserName = "taken@example.com",
            DisplayName = "existing",
            CreatedAt = Now.AddDays(-1),
            UpdatedAt = Now.AddDays(-1),
        });
        await ctx.SaveChangesAsync();

        var identity = new FakeIdentityService();
        var handler = new RegisterHandler(identity, new FakeJwtTokenGenerator(), ctx, new FakeClock(Now));

        Result<LoginSuccessModel> result = await handler.Handle(
            new RegisterRequest("Taken@Example.com", "Whatever1!", null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmailInUse");
        result.Error.Type.Should().Be(ErrorType.Conflict);

        identity.CreatedUsers.Should().BeEmpty();
        ctx.EmailVerificationTokens.Should().BeEmpty();
        ctx.EmailOutboxEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Identity_create_failure_short_circuits_before_outbox_write()
    {
        using var ctx = NewDb();
        var identity = new FakeIdentityService { CreateError = Error.Validation("Password.Weak", "Too short.") };
        var handler = new RegisterHandler(identity, new FakeJwtTokenGenerator(), ctx, new FakeClock(Now));

        Result<LoginSuccessModel> result = await handler.Handle(
            new RegisterRequest("new@example.com", "short", null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Password.Weak");

        // No verification token, no outbox event — the transaction rolled back at the seam.
        ctx.EmailVerificationTokens.Should().BeEmpty();
        ctx.EmailOutboxEvents.Should().BeEmpty();
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static ApplicationDbContext NewDb()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private sealed class FakeClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    /// <summary>
    /// Hand-rolled fake for <see cref="IIdentityService"/>. Captures the user row "inserted" by the
    /// production code path and surfaces the configured <see cref="CreateError"/> when set, so the
    /// handler's failure-short-circuit behaviour is observable from the test.
    /// </summary>
    private sealed class FakeIdentityService : IIdentityService
    {
        public List<User> CreatedUsers { get; } = [];
        public List<(User User, string Role)> AssignedRoles { get; } = [];
        public Error? CreateError { get; init; }

        public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult<User?>(CreatedUsers.FirstOrDefault(u => u.Email == email));

        public Task<bool> CheckPasswordAsync(User user, string password) => Task.FromResult(true);

        public Task<IList<string>> GetRolesAsync(User user)
            => Task.FromResult<IList<string>>(
                [.. AssignedRoles.Where(r => r.User.Id == user.Id).Select(r => r.Role)]);

        public Task<Result<User>> CreateAsync(User user, string password, CancellationToken ct = default)
        {
            if (CreateError is { } err)
                return Task.FromResult(Result.Failure<User>(err));
            user.Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id;
            CreatedUsers.Add(user);
            return Task.FromResult(Result.Success(user));
        }

        public Task<Result> AssignRoleAsync(User user, string role, CancellationToken ct = default)
        {
            AssignedRoles.Add((user, role));
            return Task.FromResult(Result.Success());
        }

        public Task<Result<(User User, bool IsNew)>> FindOrCreateGoogleUserAsync(
            string email, string googleSubject, string displayName, CancellationToken ct = default)
            => throw new NotImplementedException("Not exercised by RegisterHandler.");
    }

    private sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
    {
        public AccessToken IssueToken(User user, IList<string> roles)
            => new($"jwt-for-{user.Id}", new DateTime(2026, 5, 29, 12, 0, 0, DateTimeKind.Utc));
    }
}
