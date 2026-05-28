using Coiny.Application.Abstractions.ExternalServices.Shipping;

namespace Coiny.Application.Features.Shipments.SearchNpCities;

public record NpCitiesResponse(IReadOnlyList<NpCity> Cities);
