using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Payments.CreatePaymentIntent;

public record CreatePaymentIntentRequest(Guid LotId) : IRequest<Result<CreatePaymentIntentResponse>>;
