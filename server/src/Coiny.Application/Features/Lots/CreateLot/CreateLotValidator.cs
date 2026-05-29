using Coiny.Application.Features.Lots.Validators;
using System.Text.Json;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Domain.Enums;
using FluentValidation;

namespace Coiny.Application.Features.Lots.CreateLot;

public class CreateLotValidator : AbstractValidator<CreateLotRequest>
{
    public CreateLotValidator(IDateTimeProvider clock)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(10_000);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0);

        RuleFor(x => x.Condition)
            .IsInEnum();

        RuleFor(x => x.StartingPriceUahKopiykas)
            .GreaterThanOrEqualTo(100)
            .WithMessage("Starting price must be at least 1 UAH (100 kopiykas).");

        RuleFor(x => x.EndsAt)
            .Must((_, endsAt) => endsAt >= clock.UtcNow.AddMinutes(1))
                .WithMessage("EndsAt must be at least 1 minute in the future.")
            .Must((_, endsAt) => endsAt <= clock.UtcNow.AddDays(7))
                .WithMessage("EndsAt must be within 7 days from now.");

        RuleFor(x => x.Attributes)
            .Must(LotAttributesValidator.IsJsonObject)
            .WithMessage("Attributes must be a JSON object.");
    }
}
