using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Shared;
using MediatR;

namespace Coiny.Application.Features.Auth.Me;

public record MeRequest : IRequest<Result<MeModel>>;
