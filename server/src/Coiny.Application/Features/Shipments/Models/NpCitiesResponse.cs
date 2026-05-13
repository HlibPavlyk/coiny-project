using Coiny.Application.Abstractions.Shipping;

namespace Coiny.Application.Features.Shipments.Models;

public record NpCitiesResponse(IReadOnlyList<NpCity> Cities);
