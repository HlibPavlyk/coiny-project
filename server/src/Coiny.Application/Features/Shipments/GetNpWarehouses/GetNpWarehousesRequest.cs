using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Shipments.GetNpWarehouses;

public record GetNpWarehousesRequest(string CityRef) : IRequest<Result<NpWarehousesResponse>>;
