using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Demo.CloseLotNow;

/// <summary>
/// Demo-only: short-circuit the time-based auction close trigger. <see cref="LotId"/> is route-bound.
/// Lots in non-Active status are rejected — the production close-job's idempotency guard already
/// covers re-fires, but returning 409 here keeps the demo UI honest.
/// </summary>
public record CloseLotNowRequest(Guid LotId) : IRequest<Result>;
