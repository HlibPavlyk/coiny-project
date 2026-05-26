using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Admin.Models;
using Coiny.Domain.Enums;
using MediatR;

namespace Coiny.Application.Features.Admin.Requests;

/// <summary>
/// Admin-only paginated reports listing. Sortable columns: <c>createdAt</c>, <c>resolvedAt</c>
/// (default <c>createdAt</c> Desc). Per <c>/docs/02-api-contracts.md</c> §8.
/// </summary>
public record GetReportsRequest : PageRequest, IRequest<Result<Paginated<ReportItemModel>>>
{
    public GetReportsFilters Filters { get; init; } = new();
}

public record GetReportsFilters
{
    /// <summary>Narrow to one report state. Null returns all states.</summary>
    public ReportStatus? Status { get; init; }
}


