using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Shipments.SearchNpCities;

public record SearchNpCitiesRequest(string Query) : IRequest<Result<NpCitiesResponse>>;
