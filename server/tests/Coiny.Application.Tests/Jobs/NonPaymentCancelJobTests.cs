using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Features.Lots;
using Coiny.Application.Tests.Fakes;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Jobs;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Coiny.Application.Tests.Jobs;

public class NonPaymentCancelJobTests
{
    private static readonly DateTime Now = new(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task No_expired_payments_is_a_silent_noop()
    {
        using var ctx = NewDb();
        var stripe = new FakeStripeClient();
        var job = NewJob(ctx, stripe);

        await job.RunAsync(CancellationToken.None);

        stripe.CancelPaymentIntentCalls.Should().Be(0);
    }

    [Fact]
    public async Task Cancels_expired_payment_and_flips_lot()
    {
        using var ctx = NewDb();
        Payment payment = SeedPayment(ctx, PaymentStatus.PendingAuthorization, dueAt: Now.AddHours(-1));
        Lot lot = SeedLot(ctx, payment, LotStatus.Sold);
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        var job = NewJob(ctx, stripe);

        await job.RunAsync(CancellationToken.None);

        stripe.CancelPaymentIntentCalls.Should().Be(1);
        stripe.LastCancelReason.Should().Be("abandoned");
        stripe.LastCancelledPaymentIntentId.Should().Be(payment.StripePaymentIntentId);

        Lot updatedLot = await ctx.Lots.SingleAsync(l => l.Id == lot.Id);
        updatedLot.Status.Should().Be(LotStatus.EndedNoSale);

        OutboxEvent outbox = await ctx.OutboxEvents.SingleAsync();
        outbox.EventType.Should().Be(LotEndedPayload.EventType);
        outbox.AggregateId.Should().Be(lot.Id);
    }

    [Fact]
    public async Task Skips_payments_not_yet_due()
    {
        using var ctx = NewDb();
        Payment payment = SeedPayment(ctx, PaymentStatus.PendingAuthorization, dueAt: Now.AddHours(1));
        SeedLot(ctx, payment, LotStatus.Sold);
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        var job = NewJob(ctx, stripe);

        await job.RunAsync(CancellationToken.None);

        stripe.CancelPaymentIntentCalls.Should().Be(0);
    }

    [Fact]
    public async Task Skips_already_terminal_payments()
    {
        using var ctx = NewDb();
        Payment cancelled = SeedPayment(ctx, PaymentStatus.Cancelled, dueAt: Now.AddHours(-10));
        Payment captured = SeedPayment(ctx, PaymentStatus.Captured, dueAt: Now.AddHours(-10));
        SeedLot(ctx, cancelled, LotStatus.EndedNoSale);
        SeedLot(ctx, captured, LotStatus.Sold);
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        var job = NewJob(ctx, stripe);

        await job.RunAsync(CancellationToken.None);

        stripe.CancelPaymentIntentCalls.Should().Be(0);
    }

    [Fact]
    public async Task Stripe_failure_on_one_payment_does_not_kill_batch()
    {
        using var ctx = NewDb();
        Payment failingPayment = SeedPayment(ctx, PaymentStatus.PendingAuthorization, dueAt: Now.AddHours(-2),
            stripeIntentId: "pi_will_throw");
        SeedLot(ctx, failingPayment, LotStatus.Sold);
        Payment goodPayment = SeedPayment(ctx, PaymentStatus.PendingAuthorization, dueAt: Now.AddHours(-1));
        Lot goodLot = SeedLot(ctx, goodPayment, LotStatus.Sold);
        await ctx.SaveChangesAsync();

        var stripe = new SelectivelyThrowingStripeClient(throwOn: "pi_will_throw");
        var job = NewJob(ctx, stripe);

        await job.RunAsync(CancellationToken.None);

        // Two Stripe calls attempted; one threw, one succeeded.
        stripe.AttemptedCalls.Should().Be(2);

        Lot updatedGood = await ctx.Lots.SingleAsync(l => l.Id == goodLot.Id);
        updatedGood.Status.Should().Be(LotStatus.EndedNoSale);
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static NonPaymentCancelJob NewJob(
        ApplicationDbContext ctx,
        Coiny.Application.Abstractions.Payments.IStripeClient stripe) =>
        new(ctx, stripe, new FixedClock(Now), NullLogger<NonPaymentCancelJob>.Instance);

    private static ApplicationDbContext NewDb()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Payment SeedPayment(
        ApplicationDbContext ctx,
        PaymentStatus status,
        DateTime dueAt,
        string? stripeIntentId = null)
    {
        var p = new Payment
        {
            Id = Guid.NewGuid(),
            LotId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            AmountUahKopiykas = 15000,
            AmountUsdCents = 361,
            RateUsedUahPerUsd = 41.5m,
            StripePaymentIntentId = stripeIntentId ?? "pi_" + Guid.NewGuid().ToString("N")[..10],
            Status = status,
            DueAt = dueAt,
            CreatedAt = Now.AddDays(-5),
            UpdatedAt = Now.AddDays(-5),
        };
        ctx.Payments.Add(p);
        return p;
    }

    private static Lot SeedLot(ApplicationDbContext ctx, Payment payment, LotStatus status)
    {
        var lot = new Lot
        {
            Id = payment.LotId,
            SellerId = payment.SellerId,
            CategoryId = 1,
            Title = "T",
            Description = "D",
            Condition = LotCondition.Ungraded,
            StartingPriceUahKopiykas = 100_00,
            CurrentPriceUahKopiykas = payment.AmountUahKopiykas,
            Status = status,
            StartsAt = Now.AddDays(-10),
            EndsAt = Now.AddDays(-4),
            Attributes = "{}",
            CreatedAt = Now.AddDays(-10),
            UpdatedAt = Now.AddDays(-4),
        };
        ctx.Lots.Add(lot);
        return lot;
    }

    private sealed class FixedClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed class SelectivelyThrowingStripeClient(string throwOn)
        : Coiny.Application.Abstractions.Payments.IStripeClient
    {
        public int AttemptedCalls { get; private set; }

        public decimal UahPerUsdRate => 41.5m;
        public string PublishableKey => "pk_test";

        public Task<Coiny.Application.Abstractions.Payments.StripeAccountInfo> CreateConnectAccountAsync(
            string email, CancellationToken ct) => throw new NotImplementedException();

        public Task<Coiny.Application.Abstractions.Payments.StripeAccountLink> CreateOnboardingLinkAsync(
            string accountId, CancellationToken ct) => throw new NotImplementedException();

        public Task<Coiny.Application.Abstractions.Payments.StripeAccountInfo> GetAccountAsync(
            string accountId, CancellationToken ct) => throw new NotImplementedException();

        public Task<Coiny.Application.Abstractions.Payments.StripePaymentIntentResult> CreatePaymentIntentAsync(
            long usdCents, string sellerAccountId, IDictionary<string, string> metadata,
            string idempotencyKey, CancellationToken ct) => throw new NotImplementedException();

        public Task<Coiny.Application.Abstractions.Payments.StripePaymentIntentResult> CapturePaymentIntentAsync(
            string paymentIntentId, CancellationToken ct) => throw new NotImplementedException();

        public Task<Coiny.Application.Abstractions.Payments.StripePaymentIntentResult> CancelPaymentIntentAsync(
            string paymentIntentId, string? reason, CancellationToken ct)
        {
            AttemptedCalls++;
            if (paymentIntentId == throwOn)
                throw new InvalidOperationException("Simulated Stripe failure.");
            return Task.FromResult(new Coiny.Application.Abstractions.Payments.StripePaymentIntentResult(
                paymentIntentId, "canceled", null));
        }
    }
}
