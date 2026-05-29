using FluentValidation;

namespace Coiny.Application.Features.Payments.CheckoutDetails;

public class CheckoutDetailsValidator : AbstractValidator<CheckoutDetailsRequest>
{
    public CheckoutDetailsValidator()
    {
        RuleFor(x => x.LotId).NotEmpty();

        RuleFor(x => x.Model).NotNull().SetValidator(new CheckoutDetailsBodyValidator());
    }
}

public class CheckoutDetailsBodyValidator : AbstractValidator<CheckoutDetailsModel>
{
    public CheckoutDetailsBodyValidator()
    {
        RuleFor(x => x.RecipientCityRef)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.RecipientWarehouseRef)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.RecipientCityLabel)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.RecipientWarehouseLabel)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.RecipientName)
            .NotEmpty()
            .MaximumLength(200);

        // Strict Ukrainian phone format per /docs/02-api-contracts.md §6 + /docs/06-open-questions.md B9.
        RuleFor(x => x.RecipientPhone)
            .NotEmpty()
            .Matches(@"^\+380\d{9}$")
            .WithMessage("Phone must match the format +380XXXXXXXXX (12 digits total).");
    }
}
