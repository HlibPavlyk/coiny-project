using FluentValidation;

namespace Coiny.Application.Features.Auth.VerifyEmail;

public class VerifyEmailValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
