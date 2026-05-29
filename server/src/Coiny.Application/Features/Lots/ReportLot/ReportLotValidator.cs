using FluentValidation;

namespace Coiny.Application.Features.Lots.ReportLot;

public class ReportLotValidator : AbstractValidator<ReportLotRequest>
{
    public ReportLotValidator()
    {
        RuleFor(x => x.LotId).NotEmpty();
        RuleFor(x => x.Reason).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
