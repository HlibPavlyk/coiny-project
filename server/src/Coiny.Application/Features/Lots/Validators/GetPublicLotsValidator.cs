using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Enums;
using FluentValidation;

namespace Coiny.Application.Features.Lots.Validators;

/// <summary>
/// Guards the public lot listing's visibility contract: when a status filter is supplied it must be
/// a published one (<c>Active</c>/<c>Sold</c>). Replaces the inline status check that previously
/// lived in the seller-listing handler so the privacy boundary is enforced before the handler runs.
/// </summary>
public class GetPublicLotsValidator : AbstractValidator<GetPublicLotsRequest>
{
    public GetPublicLotsValidator()
    {
        RuleFor(x => x.Filters.Status)
            .Must(status => status is LotStatus.Active or LotStatus.Sold)
            .When(x => x.Filters.Status is not null)
            .WithMessage("Only Active and Sold lots are visible on public listings.");
    }
}
