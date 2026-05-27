using Coiny.Application.Common.Results;
using Coiny.Application.Features.Payments.Models;
using Coiny.Application.Features.Payments.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Tags("Payments")]
[Route("api/v1/payments")]
[Authorize]
public class PaymentsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Idempotently create (or reuse) the caller's Stripe Connect Express account and
    /// return a short-lived onboarding URL. Frontend redirects to the URL immediately.
    /// </summary>
    [HttpPost("connect/onboard")]
    public Task<Result<ConnectOnboardResponse>> ConnectOnboard(CancellationToken ct) =>
        mediator.Send(new ConnectOnboardRequest(), ct);

    /// <summary>Refresh the caller's Stripe Connect onboarding status from Stripe.</summary>
    [HttpGet("connect/status")]
    public Task<Result<ConnectStatusResponse>> ConnectStatus(CancellationToken ct) =>
        mediator.Send(new GetConnectStatusRequest(), ct);

    /// <summary>
    /// Generate a single-use Stripe Express dashboard URL for the caller. Frontend opens
    /// the returned link in a new tab so the seller can view their balance, transfers,
    /// payouts, and bank account settings on Stripe-hosted UI.
    /// </summary>
    [HttpGet("connect/dashboard-link")]
    public Task<Result<ExpressDashboardLinkResponse>> ExpressDashboardLink(CancellationToken ct) =>
        mediator.Send(new GetExpressDashboardLinkRequest(), ct);

    /// <summary>
    /// Stage shipping recipient details on a Sold lot. Only the winning bidder may call.
    /// Creates a Shipment row in <c>PendingTtn</c> state — must come before the payment-intent step.
    /// Nested under the won lot (no payment exists yet), per the routing convention.
    /// </summary>
    [HttpPost("/api/v1/lots/{lotId:guid}/checkout-details")]
    public Task<Result> CheckoutDetails(Guid lotId, [FromBody] CheckoutDetailsBody body, CancellationToken ct) =>
        mediator.Send(new CheckoutDetailsRequest(
            lotId,
            body.RecipientCityRef,
            body.RecipientCityLabel,
            body.RecipientWarehouseRef,
            body.RecipientWarehouseLabel,
            body.RecipientName,
            body.RecipientPhone), ct);

    /// <summary>
    /// Create the Stripe PaymentIntent (USD, manual capture, destination charge to the seller's
    /// Connect account). Returns the client secret for the frontend's card-confirmation step.
    /// Nested under the won lot (no payment exists yet), per the routing convention.
    /// </summary>
    [HttpPost("/api/v1/lots/{lotId:guid}/payment-intent")]
    public Task<Result<CreatePaymentIntentResponse>> CreateIntent(Guid lotId, CancellationToken ct) =>
        mediator.Send(new CreatePaymentIntentRequest(lotId), ct);

    /// <summary>Buyer or seller view of a payment, including the linked shipment id and status.</summary>
    [HttpGet("{paymentId:guid}")]
    public Task<Result<PaymentDetailModel>> GetById(Guid paymentId, CancellationToken ct) =>
        mediator.Send(new GetPaymentByIdRequest(paymentId), ct);
}

public record CheckoutDetailsBody(
    string RecipientCityRef,
    string RecipientCityLabel,
    string RecipientWarehouseRef,
    string RecipientWarehouseLabel,
    string RecipientName,
    string RecipientPhone);
