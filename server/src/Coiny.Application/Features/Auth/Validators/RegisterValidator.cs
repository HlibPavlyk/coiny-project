using Coiny.Application.Features.Auth.Requests;
using FluentValidation;

namespace Coiny.Application.Features.Auth.Validators;

public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"\d").WithMessage("Password must contain at least one digit.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));
    }
}
