using Coiny.Domain.Enums;

namespace Coiny.Domain.Entities;

public class Shipment
{
    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }

    /// <summary>Denormalized from Payment for cheap joins.</summary>
    public Guid LotId { get; set; }

    public Guid BuyerId { get; set; }

    public Guid SellerId { get; set; }

    /// <summary>Nova Poshta TTN (waybill number). Unique.</summary>
    public string NovaPoshtaTtn { get; set; } = string.Empty;

    /// <summary>NP's internal document id returned alongside the TTN.</summary>
    public string IntDocNumber { get; set; } = string.Empty;

    public string SenderCityRef { get; set; } = string.Empty;

    public string SenderWarehouseRef { get; set; } = string.Empty;

    public string RecipientCityRef { get; set; } = string.Empty;

    public string RecipientWarehouseRef { get; set; } = string.Empty;

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
