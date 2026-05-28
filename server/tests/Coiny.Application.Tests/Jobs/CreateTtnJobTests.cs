using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Features.Shipments;
using Coiny.Application.Features.Shipments.Shared;
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

public class CreateTtnJobTests
{
    private static readonly DateTime Now = new(2026, 5, 13, 14, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Missing_payment_is_a_silent_noop()
    {
        using var ctx = NewDb();
        var job = NewJob(ctx);

        // No payment seeded — Hangfire should not retry.
        await job.RunAsync(Guid.NewGuid(), CancellationToken.None);

        (await ctx.Shipments.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Missing_shipment_is_a_silent_noop()
    {
        using var ctx = NewDb();
        Payment payment = SeedPayment(ctx);
        await ctx.SaveChangesAsync();

        var job = NewJob(ctx);
        await job.RunAsync(payment.Id, CancellationToken.None);

        (await ctx.EmailOutboxEvents.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Happy_path_creates_ttn_and_writes_outbox()
    {
        using var ctx = NewDb();
        Payment payment = SeedPayment(ctx);
        Shipment shipment = SeedShipment(ctx, payment, status: ShipmentStatus.PendingTtn);
        await ctx.SaveChangesAsync();

        var np = new FakeNovaPoshtaClient();
        var job = NewJob(ctx, np);
        await job.RunAsync(payment.Id, CancellationToken.None);

        Shipment updated = await ctx.Shipments.SingleAsync(s => s.Id == shipment.Id);
        updated.Status.Should().Be(ShipmentStatus.TtnCreated);
        updated.NovaPoshtaTtn.Should().NotBeNull().And.StartWith("204000");
        updated.IntDocNumber.Should().NotBeNullOrEmpty();
        updated.UpdatedAt.Should().Be(Now);

        EmailOutboxEvent outbox = await ctx.EmailOutboxEvents.SingleAsync();
        outbox.EventType.Should().Be(ShipmentStatusChangedPayload.EventType);
        outbox.AggregateId.Should().Be(shipment.Id);
        outbox.ProcessedAt.Should().BeNull();

        ShipmentStatusChangedPayload payload = ShipmentStatusChangedPayload.Deserialize(outbox.Payload);
        payload.PaymentId.Should().Be(payment.Id);
        payload.ShipmentId.Should().Be(shipment.Id);
        payload.Ttn.Should().Be(updated.NovaPoshtaTtn);
        payload.Status.Should().Be("TtnCreated");
        payload.PreviousStatus.Should().Be("PendingTtn");
    }

    [Fact]
    public async Task Rerun_on_already_created_shipment_is_idempotent_noop()
    {
        using var ctx = NewDb();
        Payment payment = SeedPayment(ctx);
        SeedShipment(ctx, payment, status: ShipmentStatus.TtnCreated, ttn: "204000111111");
        await ctx.SaveChangesAsync();

        var np = new FakeNovaPoshtaClient();
        var job = NewJob(ctx, np);
        await job.RunAsync(payment.Id, CancellationToken.None);

        // The original TTN must NOT be overwritten by a fresh NP call.
        Shipment updated = await ctx.Shipments.SingleAsync();
        updated.NovaPoshtaTtn.Should().Be("204000111111");
        (await ctx.EmailOutboxEvents.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task NP_exception_propagates_for_hangfire_retry()
    {
        using var ctx = NewDb();
        Payment payment = SeedPayment(ctx);
        SeedShipment(ctx, payment, status: ShipmentStatus.PendingTtn);
        await ctx.SaveChangesAsync();

        var np = new ThrowingNovaPoshtaClient();
        var job = NewJob(ctx, np);

        Func<Task> act = () => job.RunAsync(payment.Id, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Shipment must remain in PendingTtn so a retry can pick it up.
        Shipment unchanged = await ctx.Shipments.SingleAsync();
        unchanged.Status.Should().Be(ShipmentStatus.PendingTtn);
        unchanged.NovaPoshtaTtn.Should().BeNull();
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static CreateTtnJob NewJob(
        ApplicationDbContext ctx,
        Coiny.Application.Abstractions.ExternalServices.Shipping.INovaPoshtaClient? np = null) =>
        new(
            ctx,
            np ?? new FakeNovaPoshtaClient(),
            new FixedClock(Now),
            NullLogger<CreateTtnJob>.Instance);

    private static ApplicationDbContext NewDb()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Payment SeedPayment(ApplicationDbContext ctx)
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
            StripePaymentIntentId = "pi_" + Guid.NewGuid().ToString("N")[..8],
            Status = PaymentStatus.Authorized,
            DueAt = Now.AddHours(96),
            AuthorizedAt = Now.AddMinutes(-1),
            CreatedAt = Now.AddMinutes(-10),
            UpdatedAt = Now.AddMinutes(-1),
        };
        ctx.Payments.Add(p);
        return p;
    }

    private static Shipment SeedShipment(
        ApplicationDbContext ctx,
        Payment payment,
        ShipmentStatus status,
        string? ttn = null)
    {
        var s = new Shipment
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            LotId = payment.LotId,
            BuyerId = payment.BuyerId,
            SellerId = payment.SellerId,
            NovaPoshtaTtn = ttn,
            IntDocNumber = ttn is null ? null : "intdoc_" + ttn[^4..],
            RecipientCityRef = "city-kyiv",
            RecipientWarehouseRef = "wh-1",
            RecipientName = "Recipient",
            RecipientPhone = "+380501234567",
            DeclaredValueUahKopiykas = payment.AmountUahKopiykas,
            Status = status,
            CreatedAt = Now.AddMinutes(-5),
            UpdatedAt = Now.AddMinutes(-5),
        };
        ctx.Shipments.Add(s);
        return s;
    }

    private sealed class FixedClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed class ThrowingNovaPoshtaClient : Coiny.Application.Abstractions.ExternalServices.Shipping.INovaPoshtaClient
    {
        public Task<IReadOnlyList<Coiny.Application.Abstractions.ExternalServices.Shipping.NpCity>> SearchSettlementsAsync(
            string query, CancellationToken ct) =>
            throw new InvalidOperationException("not used");

        public Task<IReadOnlyList<Coiny.Application.Abstractions.ExternalServices.Shipping.NpWarehouse>> GetWarehousesAsync(
            string cityRef, CancellationToken ct) =>
            throw new InvalidOperationException("not used");

        public Task<Coiny.Application.Abstractions.ExternalServices.Shipping.NpInternetDocument> SaveInternetDocumentAsync(
            Coiny.Application.Abstractions.ExternalServices.Shipping.NpSaveDocumentRequest request, CancellationToken ct) =>
            throw new InvalidOperationException("NP simulated transport failure");

        public Task<IReadOnlyList<Coiny.Application.Abstractions.ExternalServices.Shipping.NpTrackingStatus>> GetStatusDocumentsAsync(
            IReadOnlyList<string> ttns, CancellationToken ct) =>
            throw new InvalidOperationException("not used");
    }
}
