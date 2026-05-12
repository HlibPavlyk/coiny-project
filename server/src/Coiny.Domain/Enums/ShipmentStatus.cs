namespace Coiny.Domain.Enums;

/// <summary>
/// Coiny-side mapped Nova Poshta shipment status. The mapping from raw NP status codes lives in
/// the Application layer (sprint 3 task 09).
/// </summary>
public enum ShipmentStatus
{
    TtnCreated,
    AcceptedByCarrier,
    InTransit,
    ArrivedAtDestination,
    Delivered,
    Refused,
    Returned,
    Lost,
}
