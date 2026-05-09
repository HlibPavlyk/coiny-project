using Coiny.Application.Common.Results;
using Coiny.Domain.Enums;
using MediatR;

namespace Coiny.Application.Features.Lots.Requests;

public record ReportLotRequest(Guid LotId, ReportReason Reason, string? Note) : IRequest<Result>;
