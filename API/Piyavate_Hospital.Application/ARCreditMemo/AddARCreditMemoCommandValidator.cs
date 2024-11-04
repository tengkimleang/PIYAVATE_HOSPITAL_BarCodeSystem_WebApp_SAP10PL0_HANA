
using FluentValidation;

namespace Piyavate_Hospital.Application.ARCreditMemo;

public class AddArCreditMemoCommandValidator : AbstractValidator<AddARCreditMemoCommand>
{
    public AddArCreditMemoCommandValidator()
    {
        RuleFor(x => x.CustomerCode).NotEmpty().WithMessage("VendorCode is required");
        RuleFor(x => x.Series).GreaterThan(0).WithMessage("Series is required");
        RuleFor(x => x.DocDate).NotNull().WithMessage("DocDate is required");
        RuleFor(x => x.TaxDate).NotNull().WithMessage("TaxDate   is required");
        RuleFor(x => x.Lines).NotNull().WithMessage("Item Line is required")
            .ForEach(rule => rule.ChildRules(item =>
            {
                item.RuleFor(i => i.ItemCode).NotEmpty().WithMessage("ItemCode must not be empty");
                item.RuleFor(i => i.Qty).GreaterThan(0).WithMessage("Qty should bigger than 0");
                item.RuleFor(i => i.Price).GreaterThan(0).WithMessage("Price should bigger than 0");
            }));
    }
}