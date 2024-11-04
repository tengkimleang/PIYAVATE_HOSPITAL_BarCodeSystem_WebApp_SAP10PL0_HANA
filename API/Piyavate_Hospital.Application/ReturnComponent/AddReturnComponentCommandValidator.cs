using FluentValidation;

namespace Piyavate_Hospital.Application.ReturnComponent;

public class AddReturnComponentCommandValidator: AbstractValidator<AddReturnComponentCommand>
{
    public AddReturnComponentCommandValidator()
    {
        RuleFor(x => x.Series).GreaterThan(0).WithMessage("Series is required");
        RuleFor(x => x.DocDate).NotNull().WithMessage("DocDate is required");
        RuleFor(x => x.Lines).NotNull().WithMessage("Item Line is required")
            .ForEach(rule => rule.ChildRules(item =>
            {
                item.RuleFor(i => i.DocNum).GreaterThan(0).WithMessage("OrderNo should bigger than 0");
                item.RuleFor(i => i.WhsCode).NotEmpty().WithMessage("WhsCode must not be empty");
                item.RuleFor(i => i.Qty).GreaterThan(0).WithMessage("Qty should bigger than 0");
                item.RuleFor(i => i.BaseLineNum).GreaterThanOrEqualTo(0).WithMessage("RowLine should bigger than 0");
            }));
    }
}