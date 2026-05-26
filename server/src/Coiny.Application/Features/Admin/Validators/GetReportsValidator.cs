using Coiny.Application.Features.Admin.Requests;
using FluentValidation;

namespace Coiny.Application.Features.Admin.Validators;

/// <summary>Rejects a malformed status filter before the handler runs; null (all states) is allowed.</summary>
public class GetReportsValidator : AbstractValidator<GetReportsRequest>
{
    public GetReportsValidator()
    {
        RuleFor(x => x.Filters.Status)
            .IsInEnum()
            .When(x => x.Filters.Status is not null);
    }
}
