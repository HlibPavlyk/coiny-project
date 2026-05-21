using Coiny.Application.Abstractions.Jobs;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.ExternalServices.Stripe;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Stripe;
using Xunit;

namespace Coiny.Application.Tests.ExternalServices.Stripe;

public class StripeWebhookProcessorTests
{
    private static readonly DateTime Now = new(2026, 5, 12, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Process_with_invalid_signature_returns_validation_error()
    {
        var (processor, _, _) = BuildProcessor();

        Result result = await processor.ProcessAsync(
            rawBody: "{\"id\":\"evt_test\"}",
            signatureHeader: "t=0,v1=deadbeef",
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Stripe.WebhookBadSignature");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task First_event_persists_row_and_dispatches()
    {
        var (processor, ctx, _) = BuildProcessor();
        SeedUserWithStripe(ctx, "acct_seller", onboarded: false);
        await ctx.SaveChangesAsync();

        Event ev = NewEvent("evt_acc_1", EventTypes.AccountUpdated,
            new Account { Id = "acct_seller", DetailsSubmitted = true, ChargesEnabled = true });

        Result result = await processor.IngestVerifiedAsync(ev, "{}", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        StripeWebhookEvent row = await ctx.StripeWebhookEvents.SingleAsync();
        row.ProcessedAt.Should().NotBeNull();
        row.ProcessingError.Should().BeNull();
    }

    [Fact]
    public async Task Replay_same_event_id_is_a_noop()
    {
        var (processor, ctx, jobs) = BuildProcessor();
        SeedPayment(ctx, "pi_replay", PaymentStatus.PendingAuthorization);
        await ctx.SaveChangesAsync();

        Event ev = NewEvent("evt_replay", EventTypes.PaymentIntentAmountCapturableUpdated,
            new PaymentIntent { Id = "pi_replay" });

        await processor.IngestVerifiedAsync(ev, "{}", CancellationToken.None);
        await processor.IngestVerifiedAsync(ev, "{}", CancellationToken.None);

        // Only one persisted row.
        (await ctx.StripeWebhookEvents.CountAsync()).Should().Be(1);
        // Dispatch (and thus EnqueueCreateTtn) ran exactly once.
        jobs.EnqueueCreateTtnCalls.Should().Be(1);
    }

    [Fact]
    public async Task AccountUpdated_flips_StripeOnboarded()
    {
        var (processor, ctx, _) = BuildProcessor();
        SeedUserWithStripe(ctx, "acct_x", onboarded: false);
        await ctx.SaveChangesAsync();

        Event ev = NewEvent("evt_acc", EventTypes.AccountUpdated,
            new Account { Id = "acct_x", DetailsSubmitted = true, ChargesEnabled = true });

        await processor.IngestVerifiedAsync(ev, "{}", CancellationToken.None);

        User user = await ctx.Users.SingleAsync(u => u.StripeAccountId == "acct_x");
        user.StripeOnboarded.Should().BeTrue();
    }

    [Fact]
    public async Task PaymentIntentAmountCapturableUpdated_authorizes_and_enqueues_ttn()
    {
        var (processor, ctx, jobs) = BuildProcessor();
        Payment payment = SeedPayment(ctx, "pi_auth", PaymentStatus.PendingAuthorization);
        await ctx.SaveChangesAsync();

        Event ev = NewEvent("evt_auth", EventTypes.PaymentIntentAmountCapturableUpdated,
            new PaymentIntent { Id = "pi_auth" });

        await processor.IngestVerifiedAsync(ev, "{}", CancellationToken.None);

        Payment updated = await ctx.Payments.SingleAsync(p => p.Id == payment.Id);
        updated.Status.Should().Be(PaymentStatus.Authorized);
        updated.AuthorizedAt.Should().Be(Now);
        updated.LastWebhookEventId.Should().Be("evt_auth");

        jobs.EnqueueCreateTtnCalls.Should().Be(1);
        jobs.LastCreateTtnPaymentId.Should().Be(payment.Id);
    }

    [Fact]
    public async Task PaymentIntentSucceeded_marks_captured()
    {
        var (processor, ctx, _) = BuildProcessor();
        Payment payment = SeedPayment(ctx, "pi_cap", PaymentStatus.Authorized);
        await ctx.SaveChangesAsync();

        Event ev = NewEvent("evt_cap", EventTypes.PaymentIntentSucceeded,
            new PaymentIntent { Id = "pi_cap" });

        await processor.IngestVerifiedAsync(ev, "{}", CancellationToken.None);

        Payment updated = await ctx.Payments.SingleAsync(p => p.Id == payment.Id);
        updated.Status.Should().Be(PaymentStatus.Captured);
        updated.CapturedAt.Should().Be(Now);
    }

    [Fact]
    public async Task PaymentIntentCanceled_marks_cancelled()
    {
        var (processor, ctx, _) = BuildProcessor();
        Payment payment = SeedPayment(ctx, "pi_can", PaymentStatus.PendingAuthorization);
        await ctx.SaveChangesAsync();

        Event ev = NewEvent("evt_can", EventTypes.PaymentIntentCanceled,
            new PaymentIntent { Id = "pi_can" });

        await processor.IngestVerifiedAsync(ev, "{}", CancellationToken.None);

        Payment updated = await ctx.Payments.SingleAsync(p => p.Id == payment.Id);
        updated.Status.Should().Be(PaymentStatus.Cancelled);
        updated.CancelledAt.Should().Be(Now);
    }

    [Fact]
    public async Task Late_canceled_after_captured_is_ignored()
    {
        var (processor, ctx, _) = BuildProcessor();
        Payment payment = SeedPayment(ctx, "pi_late", PaymentStatus.Captured);
        payment.CapturedAt = Now.AddHours(-1);
        await ctx.SaveChangesAsync();

        Event ev = NewEvent("evt_late_can", EventTypes.PaymentIntentCanceled,
            new PaymentIntent { Id = "pi_late" });

        await processor.IngestVerifiedAsync(ev, "{}", CancellationToken.None);

        Payment updated = await ctx.Payments.SingleAsync(p => p.Id == payment.Id);
        updated.Status.Should().Be(PaymentStatus.Captured);
        updated.CancelledAt.Should().BeNull();
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static (StripeWebhookProcessor processor, ApplicationDbContext ctx, FakeJobScheduler jobs)
        BuildProcessor()
    {
        ApplicationDbContext ctx = NewDb();
        var jobs = new FakeJobScheduler();
        var clock = new FixedClock(Now);

        IOptions<StripeOptions> opts = Options.Create(new StripeOptions
        {
            SecretKey = "sk_test_x",
            PublishableKey = "pk_test_x",
            WebhookSecret = "whsec_test_x",
            UahPerUsd = 41.5m,
            ReturnUrl = "http://localhost/return",
            RefreshUrl = "http://localhost/refresh",
        });

        var processor = new StripeWebhookProcessor(
            ctx, opts, jobs, clock,
            NullLogger<StripeWebhookProcessor>.Instance);

        return (processor, ctx, jobs);
    }

    private static ApplicationDbContext NewDb()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedUserWithStripe(ApplicationDbContext ctx, string accountId, bool onboarded)
    {
        ctx.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = $"{accountId}@test.x",
            UserName = accountId,
            DisplayName = "Test",
            StripeAccountId = accountId,
            StripeOnboarded = onboarded,
            CreatedAt = Now.AddDays(-1),
            UpdatedAt = Now.AddDays(-1),
        });
    }

    private static Payment SeedPayment(ApplicationDbContext ctx, string intentId, PaymentStatus status)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            LotId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            AmountUahKopiykas = 1_500_00,
            AmountUsdCents = 361,
            RateUsedUahPerUsd = 41.5m,
            StripePaymentIntentId = intentId,
            Status = status,
            DueAt = Now.AddHours(96),
            CreatedAt = Now.AddMinutes(-5),
            UpdatedAt = Now.AddMinutes(-5),
        };
        ctx.Payments.Add(payment);
        return payment;
    }

    private static Event NewEvent(string id, string type, IHasObject obj) => new()
    {
        Id = id,
        Type = type,
        Data = new EventData { Object = obj },
    };

    private sealed class FakeJobScheduler : IJobScheduler
    {
        public int EnqueueCreateTtnCalls { get; private set; }
        public Guid? LastCreateTtnPaymentId { get; private set; }

        public string ScheduleAuctionClose(Guid lotId, DateTime endsAtUtc) => "scheduled";
        public string ReScheduleAuctionClose(string? previousJobId, Guid lotId, DateTime endsAtUtc) => "rescheduled";

        public string EnqueueCreateTtn(Guid paymentId)
        {
            EnqueueCreateTtnCalls++;
            LastCreateTtnPaymentId = paymentId;
            return "ttn-id";
        }

        public string EnqueueCapture(Guid paymentId) => "capture-id";
        public string EnqueueCancelPayment(Guid paymentId) => "cancel-id";
    }

    private sealed class FixedClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
