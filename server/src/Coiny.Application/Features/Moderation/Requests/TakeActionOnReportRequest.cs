using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Moderation.Requests;

/// <summary>
/// Admin records that a report was actioned. This only flips the report's resolution state — the
/// actual mutation (delete lot, ban user) is a separate explicit admin call, keeping the audit trail
/// clean. <see cref="ReportId"/> comes from the route.
/// </summary>
public record TakeActionOnReportRequest(Guid ReportId, string ResolutionNote) : IRequest<Result>;
