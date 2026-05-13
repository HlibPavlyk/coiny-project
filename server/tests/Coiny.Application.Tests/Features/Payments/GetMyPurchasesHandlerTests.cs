using Coiny.Application.Abstractions.Http;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Payments.Handlers;
using Coiny.Application.Features.Payments.Models;
using Coiny.Application.Features.Payments.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Coiny.Application.Tests.Features.Payments;

public class GetMyPurchasesHandlerTests
{
    private static readonly DateTime BaseTime = new(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc);

    private readonly Guid _callerId = Guid.NewGuid();
    private readonly Guid _otherId = Guid.NewGuid();

    [Fact]
    public async Task Anonymous_caller_returns_unauthorized()
    {
        using var ctx = NewDb();
        var handler = new GetMyPurchasesHandler(ctx, new TestCurrentUser(null));

        Result<Paginated<MyPurchaseItemModel>> result = await handler.Handle(
            new GetMyPurchasesRequest(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.NotAuthenticated");
    }

    [Fact]
    public async Task Returns_only_callers_purchases()
    {
        using var ctx = NewDb();
        Lot lot1 = SeedLot(ctx, "Coin A");
        Lot lot2 = SeedLot(ctx, "Coin B");
        SeedPayment(ctx, lot1, _callerId, BaseTime.AddMinutes(-30));
        SeedPayment(ctx, lot2, _otherId, BaseTime.AddMinutes(-10)); // someone else's
        await ctx.SaveChangesAsync();

        var handler = new GetMyPurchasesHandler(ctx, new TestCurrentUser(_callerId));
        Result<Paginated<MyPurchaseItemModel>> result = await handler.Handle(
            new GetMyPurchasesRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items.Single().Lot.Title.Should().Be("Coin A");
    }

    [Fact]
    public async Task Eagerly_joins_shipment_status_when_present()
    {
        using var ctx = NewDb();
        Lot lot = SeedLot(ctx, "Coin C");
        Payment payment = SeedPayment(ctx, lot, _callerId, BaseTime);
        SeedShipment(ctx, payment, ShipmentStatus.InTransit, ttn: "204000111222");
        await ctx.SaveChangesAsync();

        var handler = new GetMyPurchasesHandler(ctx, new TestCurrentUser(_callerId));
        Result<Paginated<MyPurchaseItemModel>> result = await handler.Handle(
            new GetMyPurchasesRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        MyPurchaseItemModel row = result.Value.Items.Single();
        row.Shipment.Should().NotBeNull();
        row.Shipment!.Status.Should().Be(ShipmentStatus.InTransit);
        row.Shipment.NovaPoshtaTtn.Should().Be("204000111222");
    }

    [Fact]
    public async Task Returns_null_shipment_when_none_linked_yet()
    {
        using var ctx = NewDb();
        Lot lot = SeedLot(ctx, "Coin D");
        SeedPayment(ctx, lot, _callerId, BaseTime);
        await ctx.SaveChangesAsync();

        var handler = new GetMyPurchasesHandler(ctx, new TestCurrentUser(_callerId));
        Result<Paginated<MyPurchaseItemModel>> result = await handler.Handle(
            new GetMyPurchasesRequest(), CancellationToken.None);

        result.Value.Items.Single().Shipment.Should().BeNull();
    }

    [Fact]
    public async Task Unknown_sort_column_returns_validation_error()
    {
        using var ctx = NewDb();
        var handler = new GetMyPurchasesHandler(ctx, new TestCurrentUser(_callerId));

        Result<Paginated<MyPurchaseItemModel>> result = await handler.Handle(
            new GetMyPurchasesRequest
            {
                SortBy = [new SortByModel("amount", SortDirection.Desc)],
            },
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sort.InvalidColumn");
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
            CurrentPriceUahKopiykas = 150_00,
            Status = LotStatus.Sold,
            StartsAt = BaseTime.AddDays(-2),
            EndsAt = BaseTime.AddHours(-1),
            Attributes = "{}",
            CreatedAt = BaseTime.AddDays(-2),
            UpdatedAt = BaseTime.AddHours(-1),
        };
        ctx.Lots.Add(lot);
        return lot;
    }

    private static Payment SeedPayment(ApplicationDbContext ctx, Lot lot, Guid buyerId, DateTime createdAt)
    {
        var p = new Payment
        {
            Id = Guid.NewGuid(),
            LotId = lot.Id,
            BuyerId = buyerId,
            SellerId = lot.SellerId,
            AmountUahKopiykas = lot.CurrentPriceUahKopiykas,
            AmountUsdCents = 361,
            RateUsedUahPerUsd = 41.5m,
            StripePaymentIntentId = "pi_" + Guid.NewGuid().ToString("N")[..8],
            Status = PaymentStatus.PendingAuthorization,
            DueAt = createdAt.AddHours(96),
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };
        ctx.Payments.Add(p);
        return p;
    }

    private static void SeedShipment(ApplicationDbContext ctx, Payment payment, ShipmentStatus status, string ttn)
    {
        ctx.Shipments.Add(new Shipment
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            LotId = payment.LotId,
            BuyerId = payment.BuyerId,
            SellerId = payment.SellerId,
            NovaPoshtaTtn = ttn,
            IntDocNumber = "intdoc-" + ttn[^4..],
            RecipientCityRef = "city",
            RecipientWarehouseRef = "wh",
            RecipientName = "Recipient",
            RecipientPhone = "+380501234567",
            DeclaredValueUahKopiykas = payment.AmountUahKopiykas,
            Status = status,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.CreatedAt,
        });
    }

    private sealed class TestCurrentUser(Guid? userId) : ICurrentUserService
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => UserId.HasValue;
        public IReadOnlyList<string> Roles { get; } = [];
    }
}
