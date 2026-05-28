using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Payments.Handlers;
using Coiny.Application.Features.Payments.Models;
using Coiny.Application.Features.Payments.Requests;
using Coiny.Application.Tests.Fakes;
using Coiny.Domain.Entities;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Coiny.Application.Tests.Features.Payments;

public class ConnectOnboardHandlerTests
{
    private readonly Guid _callerId = Guid.NewGuid();

    [Fact]
    public async Task Anonymous_caller_returns_unauthorized()
    {
        using var ctx = NewDb();
        var handler = new ConnectOnboardHandler(ctx, new TestCurrentUser(null), new FakeStripeClient());

        Result<ConnectOnboardResponse> result = await handler.Handle(
            new ConnectOnboardRequest(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.NotAuthenticated");
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task First_call_creates_account_and_returns_link()
    {
        using var ctx = NewDb();
        SeedUser(ctx, _callerId, stripeAccountId: null);
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        var handler = new ConnectOnboardHandler(ctx, new TestCurrentUser(_callerId), stripe);

        Result<ConnectOnboardResponse> result = await handler.Handle(
            new ConnectOnboardRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OnboardingUrl.Should().StartWith("https://connect.stripe.com/setup/");
        stripe.CreateAccountCalls.Should().Be(1);
        stripe.CreateLinkCalls.Should().Be(1);

        User updated = await ctx.Users.SingleAsync(u => u.Id == _callerId);
        updated.StripeAccountId.Should().Be("acct_test_001");
    }

    [Fact]
    public async Task Second_call_reuses_existing_account()
    {
        using var ctx = NewDb();
        SeedUser(ctx, _callerId, stripeAccountId: "acct_already_there");
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        var handler = new ConnectOnboardHandler(ctx, new TestCurrentUser(_callerId), stripe);

        Result<ConnectOnboardResponse> first = await handler.Handle(
            new ConnectOnboardRequest(), CancellationToken.None);
        Result<ConnectOnboardResponse> second = await handler.Handle(
            new ConnectOnboardRequest(), CancellationToken.None);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();

        // No account creation on either call — the user already had StripeAccountId.
        stripe.CreateAccountCalls.Should().Be(0);
        // But each call mints a fresh short-lived onboarding link.
        stripe.CreateLinkCalls.Should().Be(2);

        User user = await ctx.Users.SingleAsync(u => u.Id == _callerId);
        user.StripeAccountId.Should().Be("acct_already_there");
    }

    [Fact]
    public async Task Unverified_email_returns_validation_error_and_does_not_call_stripe()
    {
        using var ctx = NewDb();
        ctx.Users.Add(new User
        {
            Id = _callerId,
            Email = $"{_callerId:N}@coiny.test",
            UserName = $"{_callerId:N}",
            DisplayName = "Unverified Seller",
            EmailVerified = false,
            StripeAccountId = null,
            StripeOnboarded = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        var handler = new ConnectOnboardHandler(ctx, new TestCurrentUser(_callerId), stripe);

        Result<ConnectOnboardResponse> result = await handler.Handle(
            new ConnectOnboardRequest(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Stripe.EmailNotVerified");
        result.Error.Type.Should().Be(ErrorType.Validation);
        stripe.CreateAccountCalls.Should().Be(0);
        stripe.CreateLinkCalls.Should().Be(0);
    }

    [Fact]
    public async Task Two_consecutive_calls_on_fresh_user_only_create_account_once()
    {
        using var ctx = NewDb();
        SeedUser(ctx, _callerId, stripeAccountId: null);
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        var handler = new ConnectOnboardHandler(ctx, new TestCurrentUser(_callerId), stripe);

        await handler.Handle(new ConnectOnboardRequest(), CancellationToken.None);
        await handler.Handle(new ConnectOnboardRequest(), CancellationToken.None);

        stripe.CreateAccountCalls.Should().Be(1);
        stripe.CreateLinkCalls.Should().Be(2);
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

    private static void SeedUser(ApplicationDbContext ctx, Guid id, string? stripeAccountId)
    {
        ctx.Users.Add(new User
        {
            Id = id,
            Email = $"{id:N}@coiny.test",
            UserName = $"{id:N}",
            DisplayName = "Test Seller",
            EmailVerified = true,
            StripeAccountId = stripeAccountId,
            StripeOnboarded = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
    }

    private sealed class TestCurrentUser(Guid? userId) : ICurrentUserService
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => UserId.HasValue;
        public IReadOnlyList<string> Roles { get; } = [];
    }

}
