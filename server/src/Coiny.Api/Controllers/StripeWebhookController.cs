using Coiny.Application.Common.Results;
using Coiny.Infrastructure.ExternalServices.Stripe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Tags("Stripe Webhooks")]
[Route("webhooks/stripe")]
public class StripeWebhookController(StripeWebhookProcessor processor) : ControllerBase
{
    private const string SignatureHeader = "Stripe-Signature";

    /// <summary>
    /// Single entry point for all Stripe events. Returns 200 even when dispatch fails
    /// (so Stripe doesn't retry our way out of state); only signature verification
    /// failure returns 400. Returns <see cref="IActionResult"/> directly so the global
    /// <c>ResultTransformFilter</c> doesn't wrap raw-body handling.
    /// </summary>
    [AllowAnonymous, HttpPost]
    public async Task<IActionResult> Receive(CancellationToken ct)
    {
        if (!Request.Headers.TryGetValue(SignatureHeader, out var signatureValues))
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = "Missing Stripe-Signature header.",
                Status = StatusCodes.Status400BadRequest,
                Instance = Request.Path,
            });

        string rawBody;
        Request.EnableBuffering();
        using (var reader = new StreamReader(Request.Body, leaveOpen: true))
            rawBody = await reader.ReadToEndAsync(ct);

        Result result = await processor.ProcessAsync(rawBody, signatureValues.ToString(), ct);

        if (result.IsSuccess)
            return Ok();

        return BadRequest(new ProblemDetails
        {
            Title = "Validation Error",
            Detail = result.Error.Description,
            Status = StatusCodes.Status400BadRequest,
            Instance = Request.Path,
        });
    }
}
