using FluentValidation;

namespace Piyavate_Hospital.Shared.Models.ReceiptFinishGood;

public class ReceiptFinishGoodHeaderValidator : AbstractValidator<ReceiptFinishGoodHeader>
{
    public ReceiptFinishGoodHeaderValidator()
    {
        RuleFor(x => x.Series).NotEmpty().WithMessage("Series is require");
        RuleFor(x => x.DocDate).NotEmpty().WithMessage("DocDate is require");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Lines is require")
            .ForEach(rule => rule.SetValidator(new ReceiptFinishGoodLineValidator()));
    }
}