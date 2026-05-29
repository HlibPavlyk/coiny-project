using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Payments.GetLotPaymentState;

/// <summary>
/// Returns the lot's payment-flow state for the calling user (the winning bidder). Used by
/// <c>PayLotPage</c> on mount to skip already-completed steps when a buyer returns to an
/// in-progress purchase. Lot id is route-bound.
/// </summary>
public record GetLotPaymentStateRequest(Guid LotId) : IRequest<Result<LotPaymentStateModel>>;
