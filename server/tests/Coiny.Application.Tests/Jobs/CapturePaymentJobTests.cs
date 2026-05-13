using Coiny.Application.Abstractions.Providers;
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

public class CapturePaymentJobTests
{
    private static readonly DateTime Now = new(2026, 5, 14, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Missing_payment_is_a_silent_noop()
    {
        using var ctx = NewDb();
        var stripe = new FakeStripeClient();
        var job = NewJob(ctx, stripe);

        await job.RunAsync(Guid.NewGuid(), CancellationToken.None);

        stripe.CapturePaymentIntentCalls.Should().Be(0);
    }

    [Fact]
    public async Task Captures_when_authorized_delivered_and_24h_elapsed()
    {
        using var ctx = NewDb();
        Payment payment = SeedPayment(ctx, PaymentStatus.Authorized);
        SeedShipment(ctx, payment, ShipmentStatus.Delivered, deliveredAt: Now.AddHours(-25));
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        var job = NewJob(ctx, stripe);

        await job.RunAsync(payment.Id, CancellationToken.None);

        stripe.CapturePaymentIntentCalls.Should().Be(1);
        stripe.LastCapturedPaymentIntentId.Should().Be(payment.StripePaymentIntentId);

        // The job intentionally doesn't mutate Payment.Status — webhook does.
        Payment unchanged = await ctx.Payments.SingleAsync();
        unchanged.Status.Should().Be(PaymentStatus.Authorized);
    }

    [Fact]
    public async Task Skips_when_payment_already_captured()
    {
        using var ctx = NewDb();
        Payment payment = SeedPayment(ctx, PaymentStatus.Captured);
        SeedShipment(ctx, payment, ShipmentStatus.Delivered, deliveredAt: Now.AddHours(-25));
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        var job = NewJob(ctx, stripe);

        await job.RunAsync(payment.Id, CancellationToken.None);

        stripe.CapturePaymentIntentCalls.Should().Be(0);
    }

    [Fact]
    public async Task Skips_when_shipment_not_delivered()
    {
        using var ctx = NewDb();
        Payment payment = SeedPayment(ctx, PaymentStatus.Authorized);
        SeedShipment(ctx, payment, ShipmentStatus.InTransit, deliveredAt: null);
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        var job = NewJob(ctx, stripe);

        await job.RunAsync(payment.Id, CancellationToken.None);

        stripe.CapturePaymentIntentCalls.Should().Be(0);
    }

    [Fact]
    public async Task Skips_when_scheduled_before_24h_buffer()
    {
        using var ctx = NewDb();
        Payment payment = SeedPayment(ctx, PaymentStatus.Authorized);
        SeedShipment(ctx, payment, ShipmentStatus.Delivered, deliveredAt: Now.AddHours(-1));
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        var job = NewJob(ctx, stripe);

        await job.RunAsync(payment.Id, CancellationToken.None);

        stripe.CapturePaymentIntentCalls.Should().Be(0);
    }

    [Fact]
    public async Task Skips_when_payment_failed()
    {
        using var ctx = NewDb();
        Payment payment = SeedPayment(ctx, PaymentStatus.Failed);
        SeedShipment(ctx, payment, ShipmentStatus.Delivered, deliveredAt: Now.AddHours(-30));
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        var job = NewJob(ctx, stripe);

        await job.RunAsync(payment.Id, CancellationToken.None);

        stripe.CapturePaymentIntentCalls.Should().Be(0);
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static CapturePaymentJob NewJob(ApplicationDbContext ctx, FakeStripeClient stripe) =>
        new(ctx, stripe, new FixedClock(Now), NullLogger<CapturePaymentJob>.Instance);

    private static ApplicationDbContext NewDb()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Payment SeedPayment(ApplicationDbContext ctx, PaymentStatus status)
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
            StripePaymentIntentId = "pi_" + Guid.NewGuid().ToString("N")[..10],
            Status = status,
            DueAt = Now.AddHours(96),
            AuthorizedAt = Now.AddDays(-2),
            CreatedAt = Now.AddDays(-2),
            UpdatedAt = Now.AddDays(-2),
        };
        ctx.Payments.Add(p);
        return p;
    }

    private static void SeedShipment(
        ApplicationDbContext ctx,
        Payment payment,
        ShipmentStatus status,
        DateTime? deliveredAt)
    {
        ctx.Shipments.Add(new Shipment
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            LotId = payment.LotId,
            BuyerId = payment.BuyerId,
            SellerId = payment.SellerId,
            NovaPoshtaTtn = "204000111222",
            IntDocNumber = "intdoc-x",
            RecipientCityRef = "city",
            RecipientWarehouseRef = "wh",
            RecipientName = "Buyer",
            RecipientPhone = "+380501234567",
            DeclaredValueUahKopiykas = payment.AmountUahKopiykas,
            Status = status,
            DeliveredAt = deliveredAt,
            CreatedAt = Now.AddDays(-2),
            UpdatedAt = Now.AddDays(-1),
        });
    }

    private sealed class FixedClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
