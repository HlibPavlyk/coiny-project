using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Features.Auctions;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Jobs;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Coiny.Application.Features.Auctions.Events;

namespace Coiny.Application.Tests.Jobs;

public class PaymentReminderJobTests
{
    private static readonly DateTime Now = new(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Inserts_reminder_for_payment_in_48h_window()
    {
        using var ctx = NewDb();
        Payment payment = SeedPayment(ctx, dueAt: Now.AddHours(48));
        SeedLot(ctx, payment, "Vintage Coin");
        await ctx.SaveChangesAsync();

        var job = NewJob(ctx);
        await job.RunAsync(CancellationToken.None);

        EmailOutboxEvent outbox = await ctx.EmailOutboxEvents.SingleAsync();
        outbox.EventType.Should().Be(AuctionWonPayReminderPayload.EventType);

        var payload = AuctionWonPayReminderPayload.Deserialize(outbox.Payload);
        payload.PaymentId.Should().Be(payment.Id);
        payload.LotTitle.Should().Be("Vintage Coin");
        payload.AmountUahKopiykas.Should().Be(payment.AmountUahKopiykas);

        Payment updated = await ctx.Payments.SingleAsync();
        updated.ReminderSentAt.Should().Be(Now);
    }

    [Fact]
    public async Task Rerun_in_same_window_does_not_duplicate()
    {
        using var ctx = NewDb();
        Payment payment = SeedPayment(ctx, dueAt: Now.AddHours(48));
        SeedLot(ctx, payment, "T");
        await ctx.SaveChangesAsync();

        var job = NewJob(ctx);
        await job.RunAsync(CancellationToken.None);
        await job.RunAsync(CancellationToken.None);

        (await ctx.EmailOutboxEvents.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Skips_payments_outside_window()
    {
        using var ctx = NewDb();
        SeedPayment(ctx, dueAt: Now.AddHours(40));  // too early (< 47h away)
        SeedPayment(ctx, dueAt: Now.AddHours(60));  // too late (> 49h away)
        SeedPayment(ctx, dueAt: Now.AddHours(10));  // too late (< 47h)
        await ctx.SaveChangesAsync();

        var job = NewJob(ctx);
        await job.RunAsync(CancellationToken.None);

        (await ctx.EmailOutboxEvents.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Skips_payments_not_in_pending_authorization()
    {
        using var ctx = NewDb();
        SeedPayment(ctx, dueAt: Now.AddHours(48), status: PaymentStatus.Captured);
        SeedPayment(ctx, dueAt: Now.AddHours(48), status: PaymentStatus.Cancelled);
        SeedPayment(ctx, dueAt: Now.AddHours(48), status: PaymentStatus.Authorized);
        await ctx.SaveChangesAsync();

        var job = NewJob(ctx);
        await job.RunAsync(CancellationToken.None);

        (await ctx.EmailOutboxEvents.CountAsync()).Should().Be(0);
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static PaymentReminderJob NewJob(ApplicationDbContext ctx) =>
        new(ctx, new FixedClock(Now), NullLogger<PaymentReminderJob>.Instance);

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
        DateTime dueAt,
        PaymentStatus status = PaymentStatus.PendingAuthorization)
    {
        var p = new Payment
        {
            Id = Guid.NewGuid(),
            LotId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            AmountUahKopiykas = 25_000,
            AmountUsdCents = 602,
            RateUsedUahPerUsd = 41.5m,
            StripePaymentIntentId = "pi_" + Guid.NewGuid().ToString("N")[..10],
            Status = status,
            DueAt = dueAt,
            CreatedAt = Now.AddHours(-1),
            UpdatedAt = Now.AddHours(-1),
        };
        ctx.Payments.Add(p);
        return p;
    }

    private static void SeedLot(ApplicationDbContext ctx, Payment payment, string title)
    {
        ctx.Lots.Add(new Lot
        {
            Id = payment.LotId,
            SellerId = payment.SellerId,
            CategoryId = 1,
            Title = title,
            Description = "D",
            Condition = LotCondition.Ungraded,
            StartingPriceUahKopiykas = 100_00,
            CurrentPriceUahKopiykas = payment.AmountUahKopiykas,
            Status = LotStatus.Sold,
            StartsAt = Now.AddDays(-2),
            EndsAt = Now.AddHours(-1),
            Attributes = "{}",
            CreatedAt = Now.AddDays(-2),
            UpdatedAt = Now.AddHours(-1),
        });
    }

    private sealed class FixedClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
