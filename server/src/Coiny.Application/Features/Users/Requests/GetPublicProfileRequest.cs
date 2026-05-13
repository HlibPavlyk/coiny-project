using Coiny.Application.Common.Results;
using Coiny.Application.Features.Users.Models;
using MediatR;

namespace Coiny.Application.Features.Users.Requests;

public record GetPublicProfileRequest(Guid UserId) : IRequest<Result<PublicProfileModel>>;
