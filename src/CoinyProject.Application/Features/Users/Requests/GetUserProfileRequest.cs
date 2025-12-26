using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.Users.Models;
using MediatR;

namespace CoinyProject.Application.Features.Users.Requests;

public record GetUserProfileRequest(Guid UserId) : IRequest<Result<UserProfileModel>>;
