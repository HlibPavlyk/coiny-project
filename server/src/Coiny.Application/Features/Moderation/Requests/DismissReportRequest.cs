using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Moderation.Requests;

/// <summary>
/// Admin dismisses a report with no further action. <see cref="ReportId"/> comes from the route;
/// the controller composes it via <c>request with { ReportId = id }</c>.
/// </summary>
public record DismissReportRequest(Guid ReportId, string? ResolutionNote) : IRequest<Result>;
