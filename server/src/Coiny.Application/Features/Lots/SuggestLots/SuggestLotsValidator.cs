using FluentValidation;

namespace Coiny.Application.Features.Lots.SuggestLots;

public class SuggestLotsValidator : AbstractValidator<SuggestLotsRequest>
{
    public SuggestLotsValidator()
    {
        // Empty is OK — the handler short-circuits to an empty list. The cap matches Title's column length.
        RuleFor(x => x.Q)
            .MaximumLength(200);
    }
}
