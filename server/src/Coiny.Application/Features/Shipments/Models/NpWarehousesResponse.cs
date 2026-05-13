using Coiny.Application.Abstractions.Shipping;

namespace Coiny.Application.Features.Shipments.Models;

public record NpWarehousesResponse(IReadOnlyList<NpWarehouse> Warehouses);
