using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.Identity.Models;
using MediatR;

namespace CoinyProject.Application.Features.Identity.Requests;

public record RegisterRequest: RegisterModel, IRequest<Result<Guid>>;
