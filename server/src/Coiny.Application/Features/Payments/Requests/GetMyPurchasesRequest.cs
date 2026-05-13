using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Payments.Models;
using MediatR;

namespace Coiny.Application.Features.Payments.Requests;

/// <summary>Authenticated caller's purchase history — payments where caller is the buyer.</summary>
public record GetMyPurchasesRequest : PageRequest, IRequest<Result<Paginated<MyPurchaseItemModel>>>;
