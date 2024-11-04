using FluentValidation;

namespace Piyavate_Hospital.Shared.Models.ReturnComponentProduction;

public class ReturnComponentProductionHeaderValidator : AbstractValidator<ReturnComponentProductionHeader>
{
    public ReturnComponentProductionHeaderValidator()
    {
        RuleFor(x => x.Series).NotEmpty().WithMessage("Series is require");
        RuleFor(x => x.DocDate).NotEmpty().WithMessage("DocDate is require");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Lines is require")
            .ForEach(rule => rule.SetValidator(new ReturnComponentProductionLineValidator()));
    }
}