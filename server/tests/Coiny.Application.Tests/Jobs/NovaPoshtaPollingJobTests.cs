using Coiny.Application.Abstractions.Infrastructure.Jobs;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Abstractions.ExternalServices.Shipping;
using Coiny.Application.Features.Shipments;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Jobs;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Coiny.Application.Features.Shipments.Events;

namespace Coiny.Application.Tests.Jobs;

public class NovaPoshtaPollingJobTests
{
    private static readonly DateTime Now = new(2026, 5, 13, 15, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Empty_active_set_is_a_silent_noop()
    {
        using var ctx = NewDb();
        var (job, _, _) = NewJob(ctx);

        await job.RunAsync(CancellationToken.None);

        (await ctx.ShipmentEvents.CountAsync()).Should().Be(0);
        (await ctx.EmailOutboxEvents.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Status_change_records_event_and_outbox_for_in_transit()
    {
        using var ctx = NewDb();
        Shipment shipment = SeedShipment(ctx, currentStatus: ShipmentStatus.TtnCreated);
        await ctx.SaveChangesAsync();

        var (job, np, _) = NewJob(ctx);
        np.SetStatus(shipment.NovaPoshtaTtn!, statusCode: 5, description: "In transit");

        await job.RunAsync(CancellationToken.None);

        Shipment updated = await ctx.Shipments.SingleAsync();
        updated.Status.Should().Be(ShipmentStatus.InTransit);
        updated.LastNpStatusCode.Should().Be(5);
        updated.LastPolledAt.Should().Be(Now);

        ShipmentEvent evt = await ctx.ShipmentEvents.SingleAsync();
        evt.Status.Should().Be(ShipmentStatus.InTransit);
        evt.NpStatusCode.Should().Be(5);

        EmailOutboxEvent outbox = await ctx.EmailOutboxEvents.SingleAsync();
        outbox.EventType.Should().Be(ShipmentStatusChangedPayload.EventType);
        var payload = ShipmentStatusChangedPayload.Deserialize(outbox.Payload);
        payload.PreviousStatus.Should().Be("TtnCreated");
        payload.Status.Should().Be("InTransit");
    }

    [Fact]
    public async Task First_delivered_sets_DeliveredAt_and_enqueues_capture_immediately()
    {
        using var ctx = NewDb();
        Shipment shipment = SeedShipment(ctx, currentStatus: ShipmentStatus.InTransit);
        await ctx.SaveChangesAsync();

        var (job, np, jobs) = NewJob(ctx);
        np.SetStatus(shipment.NovaPoshtaTtn!, statusCode: 9, description: "Delivered");

        await job.RunAsync(CancellationToken.None);

        Shipment updated = await ctx.Shipments.SingleAsync();
        updated.Status.Should().Be(ShipmentStatus.Delivered);
        updated.DeliveredAt.Should().Be(Now);

        // Captured immediately on Delivered — no scheduling delay (THESIS-SCOPE §B/§F).
        jobs.EnqueueCaptureCalls.Should().Be(1);
        jobs.LastCapturePaymentId.Should().Be(shipment.PaymentId);
        jobs.EnqueueCancelPaymentCalls.Should().Be(0);
    }

    [Theory]
    [InlineData(102, ShipmentStatus.Refused)]
    [InlineData(103, ShipmentStatus.Returned)]
    public async Task Refused_or_returned_enqueues_payment_cancel_for_refund(
        int npCode, ShipmentStatus expected)
    {
        using var ctx = NewDb();
        Shipment shipment = SeedShipment(ctx, currentStatus: ShipmentStatus.InTransit);
        await ctx.SaveChangesAsync();

        var (job, np, jobs) = NewJob(ctx);
        np.SetStatus(shipment.NovaPoshtaTtn!, statusCode: npCode);

        await job.RunAsync(CancellationToken.None);

        Shipment updated = await ctx.Shipments.SingleAsync();
        updated.Status.Should().Be(expected);

        jobs.EnqueueCancelPaymentCalls.Should().Be(1);
        jobs.LastCancelPaymentId.Should().Be(shipment.PaymentId);
        jobs.EnqueueCaptureCalls.Should().Be(0);
    }

    [Fact]
    public async Task Same_response_twice_does_not_duplicate_outbox_or_change_status()
    {
        using var ctx = NewDb();
        Shipment shipment = SeedShipment(ctx, currentStatus: ShipmentStatus.TtnCreated);
        await ctx.SaveChangesAsync();

        var (job, np, _) = NewJob(ctx);
        np.SetStatus(shipment.NovaPoshtaTtn!, statusCode: 5);

        await job.RunAsync(CancellationToken.None);
        await job.RunAsync(CancellationToken.None);

        // Both runs append a ShipmentEvent observation (audit log; dedupe is on ObservedAt as well).
        // Status only changed on the first run, so the outbox row is written once.
        (await ctx.EmailOutboxEvents.CountAsync()).Should().Be(1);
        Shipment updated = await ctx.Shipments.SingleAsync();
        updated.Status.Should().Be(ShipmentStatus.InTransit);
    }

    [Fact]
    public async Task AcceptedByCarrier_transition_does_not_write_outbox()
    {
        using var ctx = NewDb();
        Shipment shipment = SeedShipment(ctx, currentStatus: ShipmentStatus.TtnCreated);
        await ctx.SaveChangesAsync();

        var (job, np, _) = NewJob(ctx);
        np.SetStatus(shipment.NovaPoshtaTtn!, statusCode: 2);

        await job.RunAsync(CancellationToken.None);

        Shipment updated = await ctx.Shipments.SingleAsync();
        updated.Status.Should().Be(ShipmentStatus.AcceptedByCarrier);
        (await ctx.EmailOutboxEvents.CountAsync()).Should().Be(0); // only InTransit/Delivered notify the buyer
    }

    [Fact]
    public async Task Unknown_npcode_keeps_current_status()
    {
        using var ctx = NewDb();
        Shipment shipment = SeedShipment(ctx, currentStatus: ShipmentStatus.TtnCreated);
        await ctx.SaveChangesAsync();

        var (job, np, _) = NewJob(ctx);
        np.SetStatus(shipment.NovaPoshtaTtn!, statusCode: 999);

        await job.RunAsync(CancellationToken.None);

        Shipment updated = await ctx.Shipments.SingleAsync();
        updated.Status.Should().Be(ShipmentStatus.TtnCreated);
        updated.LastPolledAt.Should().Be(Now);
        (await ctx.ShipmentEvents.CountAsync()).Should().Be(0);
        (await ctx.EmailOutboxEvents.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task PendingTtn_and_terminal_shipments_are_skipped()
    {
        using var ctx = NewDb();
        SeedShipment(ctx, currentStatus: ShipmentStatus.PendingTtn);
        SeedShipment(ctx, currentStatus: ShipmentStatus.Delivered);
        SeedShipment(ctx, currentStatus: ShipmentStatus.Refused);
        await ctx.SaveChangesAsync();

        var (job, np, _) = NewJob(ctx);

        await job.RunAsync(CancellationToken.None);

        // None should have been polled — no NP call recorded.
        np.GetStatusCalls.Should().Be(0);
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static (NovaPoshtaPollingJob job, ControllableNpClient np, RecordingScheduler jobs) NewJob(
        ApplicationDbContext ctx)
    {
        var np = new ControllableNpClient();
        var jobs = new RecordingScheduler();
        var job = new NovaPoshtaPollingJob(
            ctx, np, jobs, new FixedClock(Now), NullLogger<NovaPoshtaPollingJob>.Instance);
        return (job, np, jobs);
    }

    private static ApplicationDbContext NewDb()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Shipment SeedShipment(ApplicationDbContext ctx, ShipmentStatus currentStatus)
    {
        var s = new Shipment
        {
            Id = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            LotId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            NovaPoshtaTtn = "204000" + Random.Shared.Next(100_000, 999_999).ToString(),
            IntDocNumber = "intdoc-" + Guid.NewGuid().ToString("N")[..6],
            RecipientCityRef = "city",
            RecipientWarehouseRef = "wh",
            RecipientName = "Recipient",
            RecipientPhone = "+380501234567",
            DeclaredValueUahKopiykas = 15000,
            Status = currentStatus,
            CreatedAt = Now.AddMinutes(-20),
            UpdatedAt = Now.AddMinutes(-20),
        };
        ctx.Shipments.Add(s);
        return s;
    }

    private sealed class FixedClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed class ControllableNpClient : INovaPoshtaClient
    {
        private readonly Dictionary<string, (int code, string desc)> _statuses = new();
        public int GetStatusCalls { get; private set; }

        public void SetStatus(string ttn, int statusCode, string description = "") =>
            _statuses[ttn] = (statusCode, description);

        public Task<IReadOnlyList<NpCity>> SearchSettlementsAsync(string q, CancellationToken ct) =>
            throw new NotImplementedException();

        public Task<IReadOnlyList<NpWarehouse>> GetWarehousesAsync(string r, CancellationToken ct) =>
            throw new NotImplementedException();

        public Task<NpInternetDocument> SaveInternetDocumentAsync(NpSaveDocumentRequest r, CancellationToken ct) =>
            throw new NotImplementedException();

        public Task<IReadOnlyList<NpTrackingStatus>> GetStatusDocumentsAsync(
            IReadOnlyList<string> ttns, CancellationToken ct)
        {
            GetStatusCalls++;
            var results = ttns
                .Where(t => _statuses.ContainsKey(t))
                .Select(t => new NpTrackingStatus(t, _statuses[t].code, _statuses[t].desc))
                .ToList();
            return Task.FromResult<IReadOnlyList<NpTrackingStatus>>(results);
        }
    }

    private sealed class RecordingScheduler : IJobScheduler
    {
        public int EnqueueCaptureCalls { get; private set; }
        public Guid? LastCapturePaymentId { get; private set; }
        public int EnqueueCancelPaymentCalls { get; private set; }
        public Guid? LastCancelPaymentId { get; private set; }

        public string ScheduleAuctionClose(Guid lotId, DateTime endsAtUtc) => "x";
        public string ReScheduleAuctionClose(string? previousJobId, Guid lotId, DateTime endsAtUtc) => "x";
        public string EnqueueCreateTtn(Guid paymentId) => "x";

        public string EnqueueCapture(Guid paymentId)
        {
            EnqueueCaptureCalls++;
            LastCapturePaymentId = paymentId;
            return "capture-id";
        }

        public string EnqueueCancelPayment(Guid paymentId)
        {
            EnqueueCancelPaymentCalls++;
            LastCancelPaymentId = paymentId;
            return "cancel-id";
        }
    }
}
