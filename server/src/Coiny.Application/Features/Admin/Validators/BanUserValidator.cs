using Coiny.Application.Features.Admin.Requests;
using FluentValidation;

namespace Coiny.Application.Features.Admin.Validators;

/// <summary>A ban must carry a reason (stored on the user as the audit record), bounded to 500 chars.</summary>
public class BanUserValidator : AbstractValidator<BanUserRequest>
{
    public BanUserValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
    }
}
