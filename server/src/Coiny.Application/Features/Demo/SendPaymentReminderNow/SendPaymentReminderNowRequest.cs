using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Demo.SendPaymentReminderNow;

/// <summary>
/// Demo-only: force-fire the 48h won-pay reminder for the Payment attached to <see cref="LotId"/>.
/// Lot id (not payment id) drives the demo UX so the operator only needs the lot they bid on.
/// </summary>
public record SendPaymentReminderNowRequest(Guid LotId) : IRequest<Result>;
