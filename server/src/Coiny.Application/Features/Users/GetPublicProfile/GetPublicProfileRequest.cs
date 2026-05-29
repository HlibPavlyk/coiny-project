using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Users.GetPublicProfile;

public record GetPublicProfileRequest(Guid UserId) : IRequest<Result<PublicProfileModel>>;
