using Coiny.Application.Abstractions.ExternalServices.Shipping;

namespace Coiny.Application.Features.Shipments.GetNpWarehouses;

public record NpWarehousesResponse(IReadOnlyList<NpWarehouse> Warehouses);
