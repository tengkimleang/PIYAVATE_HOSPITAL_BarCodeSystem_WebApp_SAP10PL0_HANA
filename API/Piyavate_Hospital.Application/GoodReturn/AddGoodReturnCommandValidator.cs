using FluentValidation;

namespace Piyavate_Hospital.Application.GoodReturn;
public class AddGoodReturnCommandValidator : AbstractValidator<AddGoodReturnCommand>
{
    public AddGoodReturnCommandValidator()
    {
        RuleFor(x => x.CustomerCode).NotEmpty().WithMessage("CardCode Need to Specify");
        RuleFor(x => x.Series).GreaterThan(0).WithMessage("Series is required");
        RuleFor(x => x.Lines).NotNull().WithMessage("Item Line Not Null")
            .ForEach(rule => rule.ChildRules(item =>
            {
                item.RuleFor(i => i.ItemCode).NotEmpty().WithMessage("ItemCode must not be empty");
                item.RuleFor(i => i.Qty).GreaterThan(0).WithMessage("Qty should bigger than 0");
                item.RuleFor(i => i.Price).GreaterThan(0).WithMessage("Price should bigger than 0");
            }));
    }
}

