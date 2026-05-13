using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Features.Shipments.Models.NovaPoshta;
using Coiny.Infrastructure.ExternalServices.NovaPoshta;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Coiny.Application.Tests.ExternalServices.NovaPoshta;

/// <summary>
/// Verifies the time-based state machine that <see cref="HybridNovaPoshtaClient"/> uses to
/// simulate NP shipment progression for the thesis demo. The dictionary that holds simulation
/// state is process-static; we use distinct TTN values per test to keep them isolated.
/// </summary>
public class HybridNovaPoshtaClientTests
{
    private static readonly DateTime BaseTime = new(2026, 5, 13, 10, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(0,   1)]   // Just created → "registered"
    [InlineData(1,   1)]   // Still in 0–2 min window
    [InlineData(2,   2)]   // 2 min in → "removed from sender city"
    [InlineData(3,   2)]
    [InlineData(4,   5)]   // 4 min in → "in transit"
    [InlineData(5,   5)]
    [InlineData(6,   7)]   // 6 min in → "arrived at destination warehouse"
    [InlineData(7,   7)]
    [InlineData(8,   9)]   // 8 min in → "delivered"
    [InlineData(30,  9)]   // Stays at "delivered" forever
    public async Task Simulated_status_progresses_with_time(int minutesElapsed, int expectedCode)
    {
        var clock = new TestClock(BaseTime);
        var client = NewClient(clock);

        // Seed the TTN at BaseTime.
        NpInternetDocument doc = await client.SaveInternetDocumentAsync(BuildRequest(), CancellationToken.None);

        // Advance the clock and poll.
        clock.Set(BaseTime.AddMinutes(minutesElapsed));
        IReadOnlyList<NpTrackingStatus> statuses = await client.GetStatusDocumentsAsync(
            [doc.Ttn], CancellationToken.None);

        statuses.Should().HaveCount(1);
        statuses[0].Ttn.Should().Be(doc.Ttn);
        statuses[0].StatusCode.Should().Be(expectedCode);
    }

    [Fact]
    public async Task SaveInternetDocument_returns_synthetic_ttn_starting_with_204000()
    {
        var client = NewClient(new TestClock(BaseTime));

        NpInternetDocument doc = await client.SaveInternetDocumentAsync(
            BuildRequest(), CancellationToken.None);

        doc.Ttn.Should().StartWith("204000");
        doc.Ttn.Length.Should().Be(12); // "204000" + 6 digits
        doc.IntDocNumber.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Unknown_ttn_is_reseeded_and_starts_at_code_1()
    {
        // Process-restart scenario: TTN exists in DB but state dictionary lost it.
        var clock = new TestClock(BaseTime);
        var client = NewClient(clock);

        IReadOnlyList<NpTrackingStatus> statuses = await client.GetStatusDocumentsAsync(
            ["204000_never_seen_before_" + Guid.NewGuid().ToString("N")[..6]],
            CancellationToken.None);

        statuses.Should().HaveCount(1);
        statuses[0].StatusCode.Should().Be(1);
    }

    [Fact]
    public async Task Batch_lookup_returns_one_row_per_input_ttn()
    {
        var clock = new TestClock(BaseTime);
        var client = NewClient(clock);

        NpInternetDocument doc1 = await client.SaveInternetDocumentAsync(BuildRequest(), CancellationToken.None);
        NpInternetDocument doc2 = await client.SaveInternetDocumentAsync(BuildRequest(), CancellationToken.None);

        clock.Set(BaseTime.AddMinutes(5)); // both should be at code 5 ("in transit")

        IReadOnlyList<NpTrackingStatus> statuses = await client.GetStatusDocumentsAsync(
            [doc1.Ttn, doc2.Ttn], CancellationToken.None);

        statuses.Should().HaveCount(2);
        statuses.Select(s => s.Ttn).Should().BeEquivalentTo([doc1.Ttn, doc2.Ttn]);
        statuses.Should().AllSatisfy(s => s.StatusCode.Should().Be(5));
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static HybridNovaPoshtaClient NewClient(IDateTimeProvider clock)
    {
        // The real NovaPoshtaClient injected here would only be called for cities/warehouses
        // lookups, which aren't exercised in these tests. Passing a stub HttpClient keeps DI happy.
        var realClient = new NovaPoshtaClient(
            new HttpClient(),
            Microsoft.Extensions.Options.Options.Create(new NovaPoshtaOptions
            {
                ApiKey = "test",
                BaseUrl = "http://localhost/",
                PlatformSender = new PlatformSenderOptions
                {
                    Name = "test", Phone = "+380000000000",
                    CityRef = "city", WarehouseRef = "wh",
                },
            }));

        return new HybridNovaPoshtaClient(realClient, clock, NullLogger<HybridNovaPoshtaClient>.Instance);
    }

    private static NpSaveDocumentRequest BuildRequest() => new(
        SenderName: "Coiny",
        SenderPhone: "+380441234567",
        SenderCityRef: "city-sender",
        SenderWarehouseRef: "wh-sender",
        RecipientName: "Buyer",
        RecipientPhone: "+380501234567",
        RecipientCityRef: "city-recipient",
        RecipientWarehouseRef: "wh-recipient",
        DeclaredValueUah: 150m,
        Description: "Coin lot");

    private sealed class TestClock(DateTime initial) : IDateTimeProvider
    {
        private DateTime _now = initial;
        public DateTime UtcNow => _now;
        public void Set(DateTime now) => _now = now;
    }
}
