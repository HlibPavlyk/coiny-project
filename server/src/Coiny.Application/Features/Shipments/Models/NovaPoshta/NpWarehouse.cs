namespace Coiny.Application.Features.Shipments.Models.NovaPoshta;

/// <summary>
/// Nova Poshta warehouse (branch office). <see cref="Ref"/> is the NP id;
/// <see cref="Number"/> is the human-facing branch number (e.g., "Branch #14").
/// </summary>
public record NpWarehouse(
    string Ref,
    string Number,
    string Address);
