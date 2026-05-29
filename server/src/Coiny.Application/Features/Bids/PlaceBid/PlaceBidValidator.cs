using FluentValidation;

namespace Coiny.Application.Features.Bids.PlaceBid;

/// <summary>
/// Synchronous, no-DB validation. The min-increment check requires the locked Lot row's current price,
/// so it lives in <c>PlaceBidHandler</c> after the SELECT FOR UPDATE — not here.
/// </summary>
public class PlaceBidValidator : AbstractValidator<PlaceBidRequest>
{
    public PlaceBidValidator()
    {
        RuleFor(x => x.AmountUahKopiykas)
            .GreaterThan(0)
            .WithMessage("Bid amount must be greater than zero.");
    }
}
