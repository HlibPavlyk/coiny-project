using Coiny.Application.Abstractions.ExternalServices.Shipping;

namespace Coiny.Application.Features.Shipments.Models;

public record NpCitiesResponse(IReadOnlyList<NpCity> Cities);
