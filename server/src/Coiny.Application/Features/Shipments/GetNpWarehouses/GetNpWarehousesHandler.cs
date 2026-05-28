using Coiny.Application.Abstractions.ExternalServices.Shipping;
using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Shipments.GetNpWarehouses;

public class GetNpWarehousesHandler(INovaPoshtaClient np)
    : IRequestHandler<GetNpWarehousesRequest, Result<NpWarehousesResponse>>
{
    public async Task<Result<NpWarehousesResponse>> Handle(GetNpWarehousesRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.CityRef))
            return Result.Failure<NpWarehousesResponse>(
                Error.Validation("NovaPoshta.CityRefRequired", "cityRef query parameter is required."));

        try
        {
            IReadOnlyList<NpWarehouse> warehouses = await np.GetWarehousesAsync(request.CityRef.Trim(), ct);
            return Result.Success(new NpWarehousesResponse(warehouses));
        }
        catch (Exception ex)
        {
            return Result.Failure<NpWarehousesResponse>(
                Error.Internal("NovaPoshta.Unavailable", ex.Message));
        }
    }
}
