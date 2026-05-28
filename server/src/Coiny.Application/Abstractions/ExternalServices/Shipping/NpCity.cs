namespace Coiny.Application.Abstractions.ExternalServices.Shipping;

/// <summary>
/// Settlement (city / village / town) returned by Nova Poshta <c>Address/searchSettlements</c>.
/// <see cref="Ref"/> is the opaque NP identifier used everywhere downstream.
/// </summary>
public record NpCity(
    string Ref,
    string Name,
    string Area);
