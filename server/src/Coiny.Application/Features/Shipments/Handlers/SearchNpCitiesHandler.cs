using Coiny.Application.Abstractions.Shipping;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Shipments.Models;
using Coiny.Application.Features.Shipments.Requests;
using MediatR;

namespace Coiny.Application.Features.Shipments.Handlers;

public class SearchNpCitiesHandler(INovaPoshtaClient np)
    : IRequestHandler<SearchNpCitiesRequest, Result<NpCitiesResponse>>
{
    public async Task<Result<NpCitiesResponse>> Handle(SearchNpCitiesRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return Result.Success(new NpCitiesResponse(Array.Empty<NpCity>()));

        try
        {
            IReadOnlyList<NpCity> cities = await np.SearchSettlementsAsync(request.Query.Trim(), ct);
            return Result.Success(new NpCitiesResponse(cities));
        }
        catch (Exception ex)
        {
            // NovaPoshtaException + transport errors collapse into a generic upstream-unavailable failure.
            // Maps to HTTP 500 today (no UpstreamError type in the error→status table — accepted simplification
            // per /docs/02-api-contracts.md §6 note).
            return Result.Failure<NpCitiesResponse>(
                Error.Internal("NovaPoshta.Unavailable", ex.Message));
        }
    }
}
