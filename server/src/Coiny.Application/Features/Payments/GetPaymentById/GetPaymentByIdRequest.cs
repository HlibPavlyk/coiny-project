using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Payments.GetPaymentById;

public record GetPaymentByIdRequest(Guid PaymentId) : IRequest<Result<PaymentDetailModel>>;
