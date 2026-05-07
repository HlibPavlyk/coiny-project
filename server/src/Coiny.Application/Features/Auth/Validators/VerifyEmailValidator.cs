using Coiny.Application.Features.Auth.Requests;
using FluentValidation;

namespace Coiny.Application.Features.Auth.Validators;

public class VerifyEmailValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
