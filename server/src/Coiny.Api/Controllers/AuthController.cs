using System.Security.Claims;
using Coiny.Api.Authentication;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Models;
using Coiny.Application.Features.Auth.Requests;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Tags("Auth")]
public class AuthController(IMediator mediator, IConfiguration configuration) : ControllerBase
{
    /// <summary>Register a new user with email + password and send a verification email.</summary>
    [HttpPost("register")]
    public async Task<Result<MeModel>> Register([FromBody] RegisterRequest request, CancellationToken ct) =>
        await DispatchAndIssueCookieAsync(request, ct);

    /// <summary>Authenticate with email + password. Sets the auth cookie on success.</summary>
    [HttpPost("login")]
    public async Task<Result<MeModel>> Login([FromBody] LoginRequest request, CancellationToken ct) =>
        await DispatchAndIssueCookieAsync(request, ct);

    /// <summary>Clear the auth cookie. Always returns 200.</summary>
    [HttpPost("logout")]
    public Result Logout()
    {
        CookieAuthHelper.ClearAuthCookie(HttpContext);
        return Result.Success();
    }

    /// <summary>Consume a single-use verification token from the email link.</summary>
    [HttpPost("verify-email")]
    public Task<Result> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken ct) =>
        mediator.Send(request, ct);

    /// <summary>Issue a fresh verification email; rate-limited to once per minute.</summary>
    [Authorize, HttpPost("verify-email/resend")]
    public Task<Result> ResendVerification(CancellationToken ct) =>
        mediator.Send(new ResendVerificationRequest(), ct);

    /// <summary>Begin the Google OIDC challenge. Redirects to Google's consent page.</summary>
    [HttpGet("google")]
    public IActionResult GoogleChallenge([FromQuery] string? next)
    {
        AuthenticationProperties props = new()
        {
            RedirectUri = Url.Action(nameof(GoogleCallback)) ?? "/api/v1/auth/google/callback",
        };
        if (!string.IsNullOrWhiteSpace(next))
            props.Items["next"] = next;
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>OIDC redirect target. Issues the auth cookie and 302s to the frontend callback.</summary>
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback(CancellationToken ct)
    {
        var frontendBase = (configuration["Frontend:BaseUrl"] ?? string.Empty).TrimEnd('/');

        AuthenticateResult authResult = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
        if (!authResult.Succeeded || authResult.Principal is null)
            return Redirect($"{frontendBase}/auth/callback?ok=0&reason=auth_failed");

        var sub = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = authResult.Principal.FindFirstValue(ClaimTypes.Email);
        var name = authResult.Principal.FindFirstValue(ClaimTypes.Name) ?? email ?? string.Empty;
        var emailVerified = bool.TryParse(authResult.Principal.FindFirstValue("email_verified"), out var ev) && ev;

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(email))
            return Redirect($"{frontendBase}/auth/callback?ok=0&reason=missing_claims");

        Result<LoginSuccessModel> result = await mediator.Send(
            new GoogleSignInRequest(sub, email, name, emailVerified), ct);

        if (result.IsFailure)
            return Redirect($"{frontendBase}/auth/callback?ok=0&reason={Uri.EscapeDataString(result.Error.Code)}");

        CookieAuthHelper.IssueAuthCookie(HttpContext, result.Value.Token.Token, configuration);
        return Redirect($"{frontendBase}/auth/callback?ok=1");
    }

    private async Task<Result<MeModel>> DispatchAndIssueCookieAsync<TRequest>(TRequest request, CancellationToken ct)
        where TRequest : IRequest<Result<LoginSuccessModel>>
    {
        Result<LoginSuccessModel> result = await mediator.Send(request, ct);
        if (result.IsFailure)
            return Result.Failure<MeModel>(result.Error);

        CookieAuthHelper.IssueAuthCookie(HttpContext, result.Value.Token.Token, configuration);
        return Result.Success(result.Value.Me);
    }
}
