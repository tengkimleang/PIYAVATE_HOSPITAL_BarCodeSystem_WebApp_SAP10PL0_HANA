using FluentValidation;

namespace Piyavate_Hospital.Shared.Models.IssueForProduction;

public class IssueProductionHeaderValidator : AbstractValidator<IssueProductionHeader>
{
    public IssueProductionHeaderValidator()
    {
        RuleFor(x => x.Series).NotEmpty().WithMessage("Series is require");
        RuleFor(x => x.DocDate).NotEmpty().WithMessage("DocDate is require");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Lines is require")
            .ForEach(rule => rule.SetValidator(new IssueProductionLineValidator()));
    }
}