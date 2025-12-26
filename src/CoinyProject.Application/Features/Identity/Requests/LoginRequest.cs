using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.Identity.Models;
using MediatR;

namespace CoinyProject.Application.Features.Identity.Requests;

public record LoginRequest : LoginModel, IRequest<Result<LoginResponseModel>>;
