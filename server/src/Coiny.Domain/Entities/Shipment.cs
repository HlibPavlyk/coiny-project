using Coiny.Domain.Enums;

namespace Coiny.Domain.Entities;

public class Shipment
{
    public Guid Id { get; set; }

    /// <summary>
    /// FK to <see cref="Payment"/>. Null while <see cref="Status"/> is <see cref="ShipmentStatus.PendingTtn"/>
    /// and the buyer has submitted checkout details but not yet created a PaymentIntent.
    /// </summary>
    public Guid? PaymentId { get; set; }

    /// <summary>Denormalized from Payment for cheap joins.</summary>
    public Guid LotId { get; set; }

    public Guid BuyerId { get; set; }

    public Guid SellerId { get; set; }

    /// <summary>Nova Poshta TTN (waybill number). Unique. Null while <see cref="Status"/> is <see cref="ShipmentStatus.PendingTtn"/>.</summary>
    public string? NovaPoshtaTtn { get; set; }

    /// <summary>NP's internal document id returned alongside the TTN. Null while in <see cref="ShipmentStatus.PendingTtn"/>.</summary>
    public string? IntDocNumber { get; set; }

    public string SenderCityRef { get; set; } = string.Empty;

    public string SenderWarehouseRef { get; set; } = string.Empty;

    public string RecipientCityRef { get; set; } = string.Empty;

    public string RecipientWarehouseRef { get; set; } = string.Empty;

    /// <summary>Display label for <see cref="RecipientCityRef"/>, captured at checkout-details time.
    /// Denormalized so the shipment read endpoint doesn't need to round-trip NP to render the address.</summary>
    public string RecipientCityLabel { get; set; } = string.Empty;

    /// <summary>Display label for <see cref="RecipientWarehouseRef"/>, captured at checkout-details time.</summary>
    public string RecipientWarehouseLabel { get; set; } = string.Empty;

    public string RecipientName { get; set; } = string.Empty;

    /// <summary>Ukrainian phone format <c>+380…</c>.</summary>
    public string RecipientPhone { get; set; } = string.Empty;

    public long DeclaredValueUahKopiykas { get; set; }

    public ShipmentStatus Status { get; set; }

    /// <summary>Raw NP status code from the last successful poll.</summary>
    public int LastNpStatusCode { get; set; }

    /// <summary>UTC. Set when status flips to <see cref="ShipmentStatus.Delivered"/>. Drives the 24h capture buffer.</summary>
    public DateTime? DeliveredAt { get; set; }

    public DateTime? LastPolledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual ICollection<ShipmentEvent> Events { get; set; } = [];
}
