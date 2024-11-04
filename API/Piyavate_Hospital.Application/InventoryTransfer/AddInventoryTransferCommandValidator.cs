
using FluentValidation;

namespace Piyavate_Hospital.Application.InventoryTransfer;

public class AddInventoryTransferCommandValidator : AbstractValidator<AddInventoryTransferCommand>
{
    public AddInventoryTransferCommandValidator()
    {
        RuleFor(x => x.FromWarehouse).NotEmpty().WithMessage("FromWarehouse is required");
        RuleFor(x => x.ToWarehouse).NotEmpty().WithMessage("ToWarehouse is required");
        RuleFor(x => x.Series).GreaterThan(0).WithMessage("Series is required");
        RuleFor(x => x.DocDate).NotNull().WithMessage("DocDate is required");
        RuleFor(x => x.TaxDate).NotNull().WithMessage("TaxDate   is required");
        RuleFor(x => x.Lines).NotNull().WithMessage("Item Line is required")
            .ForEach(rule => rule.ChildRules(item =>
            {
                item.RuleFor(i => i.ItemCode).NotEmpty().WithMessage("ItemCode must not be empty");
                item.RuleFor(i => i.Qty).GreaterThan(0).WithMessage("Qty should bigger than 0");
            }));
    }
}