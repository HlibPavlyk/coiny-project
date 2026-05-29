using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Payments.GetPublicConfig;

public record GetPublicConfigRequest : IRequest<Result<PublicConfigModel>>;
