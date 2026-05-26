using System.Text.Json;
using Coiny.Application.Common.Json;

namespace Coiny.Application.Features.Shipments.Events;

/// <summary>
/// Payload schema for <c>EmailOutboxEvent</c> rows of type <c>ShipmentStatusChanged</c>.
/// Producer: <c>CreateTtnJob</c> (sprint 3 task 10) on TTN creation, <c>NovaPoshtaPollingJob</c>
/// (task 11) on subsequent status transitions. Consumer: <c>EmailOutboxFlushJob</c> (task 14
/// will render the actual email — only <c>InTransit</c> and <c>Delivered</c> transitions are sent).
/// </summary>
public sealed record ShipmentStatusChangedPayload(
    Guid PaymentId,
    Guid ShipmentId,
    string Ttn,
    string Status,
    string? PreviousStatus)
{
    public const string EventType = "ShipmentStatusChanged";

    public string Serialize() => JsonSerializer.Serialize(this, JsonDefaults.Options);

    public static ShipmentStatusChangedPayload Deserialize(string json) =>
        JsonSerializer.Deserialize<ShipmentStatusChangedPayload>(json, JsonDefaults.Options)
            ?? throw new InvalidOperationException("ShipmentStatusChangedPayload deserialization returned null.");
}
