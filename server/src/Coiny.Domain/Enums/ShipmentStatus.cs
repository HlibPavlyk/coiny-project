namespace Coiny.Domain.Enums;

/// <summary>
/// Coiny-side mapped Nova Poshta shipment status. The mapping from raw NP status codes lives in
/// the Application layer (sprint 3 task 09).
/// </summary>
public enum ShipmentStatus
{
    /// <summary>
    /// Buyer submitted checkout details (recipient address) but the NP TTN has not been
    /// created yet. Pre-TTN staging state. Set by the checkout-details handler.
    /// </summary>
    PendingTtn,
    TtnCreated,
    AcceptedByCarrier,
    InTransit,
    ArrivedAtDestination,
    Delivered,
    Refused,
    Returned,
    Lost,
}
