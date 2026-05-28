using FluentValidation;

namespace Coiny.Application.Features.Moderation.TakeActionOnReport;

/// <summary>Taking action requires a resolution note (the audit record), bounded to 500 chars.</summary>
public class TakeActionOnReportValidator : AbstractValidator<TakeActionOnReportRequest>
{
    public TakeActionOnReportValidator()
    {
        RuleFor(x => x.ResolutionNote)
            .NotEmpty()
            .MaximumLength(500);
    }
}
