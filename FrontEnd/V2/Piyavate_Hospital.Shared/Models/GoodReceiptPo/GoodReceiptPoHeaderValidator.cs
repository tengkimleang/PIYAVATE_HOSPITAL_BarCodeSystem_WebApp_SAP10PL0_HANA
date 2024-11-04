using FluentValidation;

namespace Piyavate_Hospital.Shared.Models.GoodReceiptPo;

public class GoodReceiptPoHeaderValidator : AbstractValidator<GoodReceiptPoHeader>
{
    public GoodReceiptPoHeaderValidator()
    {
        RuleFor(x => x.VendorCode).NotEmpty().WithMessage("Vendor is require");
        RuleFor(x => x.Series).NotEmpty().WithMessage("Series is require");
        RuleFor(x => x.DocDate).NotEmpty().WithMessage("DocDate is require");
        RuleFor(x => x.TaxDate).NotEmpty().WithMessage("TaxDate is require");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Lines is require")
            .ForEach(rule => rule.SetValidator(new GoodReceiptPoLineValidator()));
    }
}
