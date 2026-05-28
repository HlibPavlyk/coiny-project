using FluentValidation;

namespace Coiny.Application.Features.Lots.GetMyLots;

/// <summary>
/// Validates the owner-scoped lot listing's filter. Unlike the public listing there is no status
/// whitelist — the owner may view any status — so the only constraint is that a supplied status is a
/// defined enum value, rejecting malformed requests before they reach the handler.
/// </summary>
public class GetMyLotsValidator : AbstractValidator<GetMyLotsRequest>
{
    public GetMyLotsValidator()
    {
        RuleFor(x => x.Filters.Status)
            .IsInEnum()
            .When(x => x.Filters.Status is not null);
    }
}
