using Coiny.Application.Features.Admin.Requests;
using FluentValidation;

namespace Coiny.Application.Features.Admin.Validators;

/// <summary>The resolution note is optional on a dismiss, but bounded to 500 chars when supplied.</summary>
public class DismissReportValidator : AbstractValidator<DismissReportRequest>
{
    public DismissReportValidator()
    {
        RuleFor(x => x.ResolutionNote)
            .MaximumLength(500)
            .When(x => x.ResolutionNote is not null);
    }
}
