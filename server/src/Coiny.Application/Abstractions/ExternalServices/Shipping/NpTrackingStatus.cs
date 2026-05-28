namespace Coiny.Application.Abstractions.ExternalServices.Shipping;

/// <summary>
/// Single status row from <c>TrackingDocument/getStatusDocuments</c>.
/// <see cref="StatusCode"/> maps to <see cref="Coiny.Domain.Enums.ShipmentStatus"/> via StatusMapper (task 11).
/// </summary>
public record NpTrackingStatus(
    string Ttn,
    int StatusCode,
    string Description);
