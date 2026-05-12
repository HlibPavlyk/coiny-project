using Coiny.Domain.Enums;

namespace Coiny.Domain.Entities;

/// <summary>
/// Append-only audit log of every status observed on a shipment by the NP-polling job. Dedup is
/// enforced by a UNIQUE index on <c>(ShipmentId, NpStatusCode, ObservedAt)</c>.
/// </summary>
public class ShipmentEvent
{
    public long Id { get; set; }

    public Guid ShipmentId { get; set; }

    public ShipmentStatus Status { get; set; }

    public int NpStatusCode { get; set; }

    public string? Description { get; set; }

    public DateTime ObservedAt { get; set; }

    public virtual Shipment? Shipment { get; set; }
}
