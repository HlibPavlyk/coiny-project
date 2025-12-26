using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.Identity.Models;
using CoinyProject.Application.Features.Identity.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoinyProject.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator) : Controller
{
    [HttpPost("register")]
    public Task<Result<Guid>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        return mediator.Send(request, cancellationToken);
    }

    [HttpPost("login")]
    public Task<Result<LoginResponseModel>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        return mediator.Send(request, cancellationToken);
    }
}
