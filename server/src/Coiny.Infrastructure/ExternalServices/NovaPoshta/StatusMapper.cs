using Coiny.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Coiny.Infrastructure.ExternalServices.NovaPoshta;

/// <summary>
/// Converts raw Nova Poshta status codes into our internal <see cref="ShipmentStatus"/>.
/// Mapping table is locked in <c>/docs/01-data-model.md §5</c>. Code <c>7</c> overlaps
/// <see cref="ShipmentStatus.InTransit"/> and <see cref="ShipmentStatus.ArrivedAtDestination"/>;
/// per the spec footnote we pick the more advanced (<c>ArrivedAtDestination</c>).
/// Unknown codes don't change status — they log a warning and the caller treats the absence of
/// a return value as "no transition".
/// </summary>
public static class StatusMapper
{
    public static ShipmentStatus? TryMap(int npCode, ILogger? logger = null)
    {
        switch (npCode)
        {
            case 1:
                return ShipmentStatus.TtnCreated;

            case 2:
            case 3:
                return ShipmentStatus.AcceptedByCarrier;

            // Code 7 falls under both InTransit and ArrivedAtDestination in NP's docs;
            // the spec footnote tells us to pick the more advanced state.
            case 7:
                return ShipmentStatus.ArrivedAtDestination;

            case 4:
            case 5:
            case 6:
            case 8:
                return ShipmentStatus.InTransit;

            case 9:
                return ShipmentStatus.Delivered;

            case 102:
                return ShipmentStatus.Refused;

            case 103:
                return ShipmentStatus.Returned;

            case 111:
                return ShipmentStatus.Lost;

            default:
                logger?.LogWarning("StatusMapper: unknown Nova Poshta status code {Code} — keeping current status", npCode);
                return null;
        }
    }
}
