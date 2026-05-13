using Coiny.Application.Abstractions.Email;
using Coiny.Application.Features.Auctions;
using Coiny.Application.Features.Auth;
using Coiny.Application.Features.Shipments;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Jobs;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Coiny.Application.Tests.Jobs;

public class EmailOutboxFlushJobTests
{
    private const string FrontendBase = "http://localhost:5173";

    [Fact]
    public async Task Verification_email_dispatched_and_marked_processed()
    {
        using var ctx = NewDb();
        var sender = new RecordingEmailSender();

        ctx.EmailOutboxEvents.Add(new EmailOutboxEvent
        {
            AggregateType = "User",
            AggregateId = Guid.NewGuid(),
            EventType = EmailVerificationPayload.EventType,
            Payload = new EmailVerificationPayload(Guid.NewGuid(), "buyer@coiny.test", "tok-abc", "hash").Serialize(),
            CreatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();

        var job = NewJob(ctx, sender);
        await job.RunAsync(CancellationToken.None);

        sender.VerificationCalls.Should().Be(1);
        sender.LastVerificationToAddress.Should().Be("buyer@coiny.test");
        sender.LastVerificationUrl.Should().StartWith($"{FrontendBase}/verify-email?token=");

        EmailOutboxEvent row = await ctx.EmailOutboxEvents.SingleAsync();
        row.ProcessedAt.Should().NotBeNull();
        row.AttemptCount.Should().Be(0);
    }

    [Fact]
    public async Task WonPay_email_dispatched_with_payment_url()
    {
        using var ctx = NewDb();
        var sender = new RecordingEmailSender();
        User buyer = SeedUser(ctx, "winner@coiny.test");
        Lot lot = SeedLot(ctx, "Coin A");
        Payment payment = SeedPayment(ctx, buyer, lot);
        ctx.EmailOutboxEvents.Add(new EmailOutboxEvent
        {
            AggregateType = "Lot",
            AggregateId = lot.Id,
            EventType = AuctionWonPayWithin96hPayload.EventType,
            Payload = new AuctionWonPayWithin96hPayload(
                lot.Id, lot.Title, payment.AmountUahKopiykas, payment.DueAt).Serialize(),
            CreatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();

        var job = NewJob(ctx, sender);
        await job.RunAsync(CancellationToken.None);

        sender.WonPayCalls.Should().Be(1);
        sender.LastWonPayToAddress.Should().Be("winner@coiny.test");
        sender.LastWonPayUrl.Should().Be($"{FrontendBase}/my-purchases/{payment.Id}");
        sender.LastWonPayReminderFlag.Should().BeFalse();
    }

    [Fact]
    public async Task WonPayReminder_email_dispatched()
    {
        using var ctx = NewDb();
        var sender = new RecordingEmailSender();
        User buyer = SeedUser(ctx, "winner2@coiny.test");
        Lot lot = SeedLot(ctx, "Coin B");
        Payment payment = SeedPayment(ctx, buyer, lot);

        ctx.EmailOutboxEvents.Add(new EmailOutboxEvent
        {
            AggregateType = "Payment",
            AggregateId = payment.Id,
            EventType = AuctionWonPayReminderPayload.EventType,
            Payload = new AuctionWonPayReminderPayload(
                payment.Id, lot.Id, lot.Title, payment.AmountUahKopiykas, payment.DueAt).Serialize(),
            CreatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();

        var job = NewJob(ctx, sender);
        await job.RunAsync(CancellationToken.None);

        sender.WonPayCalls.Should().Be(1);
        sender.LastWonPayReminderFlag.Should().BeTrue();
        sender.LastWonPayToAddress.Should().Be("winner2@coiny.test");
    }

    [Fact]
    public async Task ShipmentStatus_inTransit_sends_email()
    {
        using var ctx = NewDb();
        var sender = new RecordingEmailSender();
        User buyer = SeedUser(ctx, "buyer3@coiny.test");
        Lot lot = SeedLot(ctx, "Coin C");
        Shipment shipment = SeedShipment(ctx, buyer, lot);

        ctx.EmailOutboxEvents.Add(new EmailOutboxEvent
        {
            AggregateType = "Shipment",
            AggregateId = shipment.Id,
            EventType = ShipmentStatusChangedPayload.EventType,
            Payload = new ShipmentStatusChangedPayload(
                shipment.PaymentId ?? Guid.NewGuid(),
                shipment.Id,
                shipment.NovaPoshtaTtn!,
                Status: "InTransit",
                PreviousStatus: "AcceptedByCarrier").Serialize(),
            CreatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();

        var job = NewJob(ctx, sender);
        await job.RunAsync(CancellationToken.None);

        sender.ShipmentCalls.Should().Be(1);
        sender.LastShipmentStatus.Should().Be("InTransit");
        sender.LastShipmentTtn.Should().Be(shipment.NovaPoshtaTtn);
        sender.LastShipmentLotTitle.Should().Be("Coin C");

        EmailOutboxEvent row = await ctx.EmailOutboxEvents.SingleAsync();
        row.ProcessedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("TtnCreated")]
    [InlineData("AcceptedByCarrier")]
    [InlineData("ArrivedAtDestination")]
    [InlineData("Refused")]
    [InlineData("Returned")]
    [InlineData("Lost")]
    public async Task ShipmentStatus_non_notifying_is_marked_processed_without_send(string status)
    {
        using var ctx = NewDb();
        var sender = new RecordingEmailSender();
        User buyer = SeedUser(ctx, "buyer4@coiny.test");
        Lot lot = SeedLot(ctx, "Coin D");
        Shipment shipment = SeedShipment(ctx, buyer, lot);

        ctx.EmailOutboxEvents.Add(new EmailOutboxEvent
        {
            AggregateType = "Shipment",
            AggregateId = shipment.Id,
            EventType = ShipmentStatusChangedPayload.EventType,
            Payload = new ShipmentStatusChangedPayload(
                shipment.PaymentId ?? Guid.NewGuid(),
                shipment.Id,
                shipment.NovaPoshtaTtn!,
                Status: status,
                PreviousStatus: "PendingTtn").Serialize(),
            CreatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();

        var job = NewJob(ctx, sender);
        await job.RunAsync(CancellationToken.None);

        sender.ShipmentCalls.Should().Be(0);

        EmailOutboxEvent row = await ctx.EmailOutboxEvents.SingleAsync();
        row.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Unknown_event_type_is_parked_with_warning_not_retried()
    {
        using var ctx = NewDb();
        var sender = new RecordingEmailSender();

        ctx.EmailOutboxEvents.Add(new EmailOutboxEvent
        {
            AggregateType = "Test",
            AggregateId = Guid.NewGuid(),
            EventType = "TotallyUnknownThing",
            Payload = "{}",
            CreatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();

        var job = NewJob(ctx, sender);
        await job.RunAsync(CancellationToken.None);

        EmailOutboxEvent row = await ctx.EmailOutboxEvents.SingleAsync();
        row.ProcessedAt.Should().NotBeNull();
        row.AttemptCount.Should().Be(0);
    }

    [Fact]
    public async Task Sender_exception_increments_attempt_count_and_keeps_row_unprocessed()
    {
        using var ctx = NewDb();
        var sender = new ThrowingEmailSender();

        ctx.EmailOutboxEvents.Add(new EmailOutboxEvent
        {
            AggregateType = "User",
            AggregateId = Guid.NewGuid(),
            EventType = EmailVerificationPayload.EventType,
            Payload = new EmailVerificationPayload(Guid.NewGuid(), "x@y.z", "tok", "hash").Serialize(),
            CreatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();

        var job = NewJob(ctx, sender);
        await job.RunAsync(CancellationToken.None);

        EmailOutboxEvent row = await ctx.EmailOutboxEvents.SingleAsync();
        row.ProcessedAt.Should().BeNull();
        row.AttemptCount.Should().Be(1);
        row.LastError.Should().NotBeNullOrEmpty();
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static EmailOutboxFlushJob NewJob(ApplicationDbContext ctx, IEmailSender sender)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Frontend:BaseUrl"] = FrontendBase })
            .Build();
        return new EmailOutboxFlushJob(ctx, sender, config, NullLogger<EmailOutboxFlushJob>.Instance);
    }

    private static ApplicationDbContext NewDb()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static User SeedUser(ApplicationDbContext ctx, string email)
    {
        var u = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            DisplayName = email[..email.IndexOf('@')],
        };
        ctx.Users.Add(u);
        return u;
    }

    private static Lot SeedLot(ApplicationDbContext ctx, string title)
    {
        var lot = new Lot
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            CategoryId = 1,
            Title = title,
            Description = "D",
            Condition = LotCondition.Ungraded,
            StartingPriceUahKopiykas = 100_00,
            CurrentPriceUahKopiykas = 100_00,
            Status = LotStatus.Sold,
            StartsAt = DateTime.UtcNow.AddDays(-1),
            EndsAt = DateTime.UtcNow,
            Attributes = "{}",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
        };
        ctx.Lots.Add(lot);
        return lot;
    }

    private static Payment SeedPayment(ApplicationDbContext ctx, User buyer, Lot lot)
    {
        var p = new Payment
        {
            Id = Guid.NewGuid(),
            LotId = lot.Id,
            BuyerId = buyer.Id,
            SellerId = Guid.NewGuid(),
            AmountUahKopiykas = 15_000,
            AmountUsdCents = 361,
            RateUsedUahPerUsd = 41.5m,
            StripePaymentIntentId = "pi_" + Guid.NewGuid().ToString("N")[..8],
            Status = PaymentStatus.PendingAuthorization,
            DueAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        ctx.Payments.Add(p);
        return p;
    }

    private static Shipment SeedShipment(ApplicationDbContext ctx, User buyer, Lot lot)
    {
        var s = new Shipment
        {
            Id = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            LotId = lot.Id,
            BuyerId = buyer.Id,
            SellerId = Guid.NewGuid(),
            NovaPoshtaTtn = "204000123456",
            IntDocNumber = "intdoc-x",
            RecipientCityRef = "city",
            RecipientWarehouseRef = "wh",
            RecipientName = "Recipient",
            RecipientPhone = "+380501234567",
            DeclaredValueUahKopiykas = 15_000,
            Status = ShipmentStatus.InTransit,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            UpdatedAt = DateTime.UtcNow,
        };
        ctx.Shipments.Add(s);
        return s;
    }

    private sealed class RecordingEmailSender : IEmailSender
    {
        public int VerificationCalls { get; private set; }
        public string? LastVerificationToAddress { get; private set; }
        public string? LastVerificationUrl { get; private set; }

        public int WonPayCalls { get; private set; }
        public string? LastWonPayToAddress { get; private set; }
        public string? LastWonPayUrl { get; private set; }
        public bool LastWonPayReminderFlag { get; private set; }

        public int ShipmentCalls { get; private set; }
        public string? LastShipmentLotTitle { get; private set; }
        public string? LastShipmentTtn { get; private set; }
        public string? LastShipmentStatus { get; private set; }

        public Task SendVerificationEmailAsync(string toAddress, string verificationUrl, CancellationToken ct)
        {
            VerificationCalls++;
            LastVerificationToAddress = toAddress;
            LastVerificationUrl = verificationUrl;
            return Task.CompletedTask;
        }

        public Task SendWonPayEmailAsync(
            string toAddress, string lotTitle, long amountUahKopiykas,
            DateTime dueAtUtc, string paymentUrl, CancellationToken ct)
        {
            WonPayCalls++;
            LastWonPayToAddress = toAddress;
            LastWonPayUrl = paymentUrl;
            LastWonPayReminderFlag = false;
            return Task.CompletedTask;
        }

        public Task SendWonPayReminderEmailAsync(
            string toAddress, string lotTitle, long amountUahKopiykas,
            DateTime dueAtUtc, string paymentUrl, CancellationToken ct)
        {
            WonPayCalls++;
            LastWonPayToAddress = toAddress;
            LastWonPayUrl = paymentUrl;
            LastWonPayReminderFlag = true;
            return Task.CompletedTask;
        }

        public Task SendShipmentStatusEmailAsync(
            string toAddress, string lotTitle, string ttn, string status, CancellationToken ct)
        {
            ShipmentCalls++;
            LastShipmentLotTitle = lotTitle;
            LastShipmentTtn = ttn;
            LastShipmentStatus = status;
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingEmailSender : IEmailSender
    {
        public Task SendVerificationEmailAsync(string toAddress, string verificationUrl, CancellationToken ct) =>
            throw new InvalidOperationException("Simulated SMTP failure.");

        public Task SendWonPayEmailAsync(
            string toAddress, string lotTitle, long amountUahKopiykas,
            DateTime dueAtUtc, string paymentUrl, CancellationToken ct) =>
            throw new InvalidOperationException("Simulated SMTP failure.");

        public Task SendWonPayReminderEmailAsync(
            string toAddress, string lotTitle, long amountUahKopiykas,
            DateTime dueAtUtc, string paymentUrl, CancellationToken ct) =>
            throw new InvalidOperationException("Simulated SMTP failure.");

        public Task SendShipmentStatusEmailAsync(
            string toAddress, string lotTitle, string ttn, string status, CancellationToken ct) =>
            throw new InvalidOperationException("Simulated SMTP failure.");
    }
}
