using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Requests;
using MediatR;

namespace Coiny.Application.Features.Auth.Handlers;

/// <summary>
/// Pass-through handler. Cookie clearance lives in the controller (task 14) via <c>CookieAuthHelper.ClearAuthCookie</c>.
/// </summary>
public class LogoutHandler : IRequestHandler<LogoutRequest, Result>
{
    public Task<Result> Handle(LogoutRequest request, CancellationToken ct) =>
        Task.FromResult(Result.Success());
}
