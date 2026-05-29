using Coiny.Domain.Enums;

namespace Coiny.Application.Features.Payments.GetLotPaymentState;

/// <summary>
/// Snapshot of where the buyer is in the post-win purchase flow. Both fields are nullable: the
/// buyer may have neither, only the shipment, or both — the frontend uses the combination to pick
/// the right step.
/// </summary>
/// <param name="ShipmentExists">True once checkout details (city + branch + recipient) are saved.</param>
/// <param name="Payment">Non-null once the Stripe PaymentIntent has been minted.</param>
public record LotPaymentStateModel(bool ShipmentExists, PaymentStateRow? Payment);

public record PaymentStateRow(Guid Id, PaymentStatus Status);
