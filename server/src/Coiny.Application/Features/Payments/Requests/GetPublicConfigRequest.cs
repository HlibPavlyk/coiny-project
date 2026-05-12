using Coiny.Application.Common.Results;
using Coiny.Application.Features.Payments.Models;
using MediatR;

namespace Coiny.Application.Features.Payments.Requests;

public record GetPublicConfigRequest : IRequest<Result<PublicConfigModel>>;
