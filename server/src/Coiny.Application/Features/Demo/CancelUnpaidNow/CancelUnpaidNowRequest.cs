using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Demo.CancelUnpaidNow;

/// <summary>Demo-only: force the 96h non-payment cancel path for the Payment attached to <see cref="LotId"/>.</summary>
public record CancelUnpaidNowRequest(Guid LotId) : IRequest<Result>;
