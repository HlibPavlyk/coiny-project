using Coiny.Domain.Enums;
using FluentValidation;

namespace Coiny.Application.Features.Lots.SearchLots;

/// <summary>
/// Guards the lot search contract: a supplied status must be a published one (<c>Active</c>/<c>Sold</c>),
/// and the price range, when both ends are given, must not be inverted.
/// </summary>
public class SearchLotsValidator : AbstractValidator<SearchLotsRequest>
{
    public SearchLotsValidator()
    {
        RuleFor(x => x.Filters.Status)
            .Must(status => status is LotStatus.Active or LotStatus.Sold)
            .When(x => x.Filters.Status is not null)
            .WithMessage("Only Active and Sold lots are visible on public listings.");

        RuleFor(x => x.Filters.PriceUahKopiykasFrom)
            .LessThanOrEqualTo(x => x.Filters.PriceUahKopiykasTo!.Value)
            .When(x => x.Filters is { PriceUahKopiykasFrom: not null, PriceUahKopiykasTo: not null })
            .WithMessage("priceUahKopiykasFrom must not exceed priceUahKopiykasTo.");
    }
}
