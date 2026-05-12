using Coiny.Application.Abstractions.Http;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Payments.Handlers;
using Coiny.Application.Features.Payments.Models;
using Coiny.Application.Features.Payments.Requests;
using Coiny.Application.Tests.Fakes;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Coiny.Application.Tests.Features.Payments;

public class CreatePaymentIntentHandlerTests
{
    private static readonly DateTime BaseTime = new(2026, 5, 12, 12, 0, 0, DateTimeKind.Utc);

    private readonly Guid _winnerId = Guid.NewGuid();
    private readonly Guid _sellerId = Guid.NewGuid();
    private readonly Guid _outsiderId = Guid.NewGuid();

    [Fact]
    public async Task Non_winner_caller_is_forbidden()
    {
        using var ctx = NewDb();
        var seedIds = await Seed(ctx, lotEndsAt: BaseTime.AddHours(-1), withShipment: true);

        var handler = NewHandler(ctx, _outsiderId);
        Result<CreatePaymentIntentResponse> result = await handler.Handle(
            new CreatePaymentIntentRequest(seedIds.LotId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Lot.NotWinner");
        result.Error.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Missing_checkout_details_returns_400()
    {
        using var ctx = NewDb();
        var seedIds = await Seed(ctx, lotEndsAt: BaseTime.AddHours(-1), withShipment: false);

        var handler = NewHandler(ctx, _winnerId);
        Result<CreatePaymentIntentResponse> result = await handler.Handle(
            new CreatePaymentIntentRequest(seedIds.LotId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Shipment.NoCheckoutDetails");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Existing_payment_returns_conflict()
    {
        using var ctx = NewDb();
        var seedIds = await Seed(ctx, lotEndsAt: BaseTime.AddHours(-1), withShipment: true);

        ctx.Payments.Add(new Payment
        {
            Id = Guid.NewGuid(),
            LotId = seedIds.LotId,
            BuyerId = _winnerId,
            SellerId = _sellerId,
            AmountUahKopiykas = 1_500_00,
            AmountUsdCents = 361,
            RateUsedUahPerUsd = 41.5m,
            StripePaymentIntentId = "pi_prev",
            Status = PaymentStatus.PendingAuthorization,
            DueAt = BaseTime.AddHours(95),
            CreatedAt = BaseTime,
            UpdatedAt = BaseTime,
        });
        await ctx.SaveChangesAsync();

        var handler = NewHandler(ctx, _winnerId);
        Result<CreatePaymentIntentResponse> result = await handler.Handle(
            new CreatePaymentIntentRequest(seedIds.LotId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.AlreadyExists");
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Happy_path_creates_payment_and_links_shipment()
    {
        using var ctx = NewDb();
        // Winning bid = 4150 kop (41.50 UAH). At rate 41.5 → exactly $1.00 = 100 cents.
        var seedIds = await Seed(
            ctx,
            lotEndsAt: BaseTime,
            withShipment: true,
            winningAmountUah: 4_150L);

        var stripe = new FakeStripeClient
        {
            UahPerUsdRate = 41.5m,
            PublishableKey = "pk_test_xyz",
            NextPaymentIntentId = "pi_happy",
            NextPaymentIntentClientSecret = "pi_happy_secret_abc",
        };

        var handler = NewHandler(ctx, _winnerId, stripe);
        Result<CreatePaymentIntentResponse> result = await handler.Handle(
            new CreatePaymentIntentRequest(seedIds.LotId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ClientSecret.Should().Be("pi_happy_secret_abc");
        result.Value.PublishableKey.Should().Be("pk_test_xyz");
        result.Value.AmountUahKopiykasDisplay.Should().Be(4_150L);
        result.Value.AmountUsdCentsCharged.Should().Be(100L);
        result.Value.RateUsedUahPerUsd.Should().Be(41.5m);

        stripe.CreatePaymentIntentCalls.Should().Be(1);
        stripe.LastIdempotencyKey.Should().Be(seedIds.LotId.ToString());
        stripe.LastUsdCents.Should().Be(100L);
        stripe.LastSellerAccountId.Should().Be("acct_seller_001");
        stripe.LastMetadata.Should().ContainKey("lotId").And.ContainKey("paymentId");

        Payment payment = await ctx.Payments.SingleAsync();
        payment.LotId.Should().Be(seedIds.LotId);
        payment.AmountUahKopiykas.Should().Be(4_150L);
        payment.AmountUsdCents.Should().Be(100L);
        payment.StripePaymentIntentId.Should().Be("pi_happy");
        payment.Status.Should().Be(PaymentStatus.PendingAuthorization);
        payment.DueAt.Should().Be(BaseTime.AddHours(96));

        Shipment shipment = await ctx.Shipments.SingleAsync();
        shipment.PaymentId.Should().Be(payment.Id);
        shipment.Status.Should().Be(ShipmentStatus.PendingTtn);
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private record SeedResult(Guid LotId);

    private async Task<SeedResult> Seed(
        ApplicationDbContext ctx,
        DateTime lotEndsAt,
        bool withShipment,
        long winningAmountUah = 1_500_00)
    {
        ctx.Users.Add(new User
        {
            Id = _winnerId,
            Email = $"{_winnerId:N}@test.x",
            UserName = $"{_winnerId:N}",
            DisplayName = "Winner",
        });
        ctx.Users.Add(new User
        {
            Id = _sellerId,
            Email = $"{_sellerId:N}@test.x",
            UserName = $"{_sellerId:N}",
            DisplayName = "Seller",
            StripeAccountId = "acct_seller_001",
            StripeOnboarded = true,
        });

        Guid lotId = Guid.NewGuid();
        Guid bidId = Guid.NewGuid();
        ctx.Lots.Add(new Lot
        {
            Id = lotId,
            SellerId = _sellerId,
            CategoryId = 1,
            Title = "T",
            Description = "D",
            Condition = LotCondition.Ungraded,
            StartingPriceUahKopiykas = 100_00,
            CurrentPriceUahKopiykas = winningAmountUah,
            BidCount = 1,
            Status = LotStatus.Sold,
            StartsAt = BaseTime.AddDays(-1),
            EndsAt = lotEndsAt,
            WinningBidId = bidId,
            Attributes = "{}",
            CreatedAt = BaseTime.AddDays(-1),
            UpdatedAt = BaseTime,
        });
        ctx.Bids.Add(new Bid
        {
            Id = bidId,
            LotId = lotId,
            BidderId = _winnerId,
            AmountUahKopiykas = winningAmountUah,
            CreatedAt = lotEndsAt.AddMinutes(-10),
        });

        if (withShipment)
        {
            ctx.Shipments.Add(new Shipment
            {
                Id = Guid.NewGuid(),
                LotId = lotId,
                BuyerId = _winnerId,
                SellerId = _sellerId,
                RecipientCityRef = "city-ref",
                RecipientWarehouseRef = "wh-ref",
                RecipientName = "Recipient",
                RecipientPhone = "+380501234567",
                DeclaredValueUahKopiykas = winningAmountUah,
                Status = ShipmentStatus.PendingTtn,
                CreatedAt = BaseTime,
                UpdatedAt = BaseTime,
            });
        }

        await ctx.SaveChangesAsync();
        return new SeedResult(lotId);
    }

    private CreatePaymentIntentHandler NewHandler(
        ApplicationDbContext ctx,
        Guid callerId,
        FakeStripeClient? stripe = null) =>
        new(
            ctx,
            new TestCurrentUser(callerId),
            new FixedClock(BaseTime),
            stripe ?? new FakeStripeClient());

    private static ApplicationDbContext NewDb()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private sealed class TestCurrentUser(Guid? userId) : ICurrentUserService
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => UserId.HasValue;
        public IReadOnlyList<string> Roles { get; } = [];
    }

    private sealed class FixedClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
