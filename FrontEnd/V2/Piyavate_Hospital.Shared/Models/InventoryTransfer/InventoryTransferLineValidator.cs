using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piyavate_Hospital.Shared.Models.InventoryTransfer;

public class InventoryTransferLineValidator : AbstractValidator<InventoryTransferLine>
{
    public InventoryTransferLineValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().WithMessage("Item Code is Require");
        RuleFor(x => x.Qty).NotNull().WithMessage("Qty is Require");
        RuleFor(x => x.Qty).GreaterThan(0).WithMessage("Qty is Require");
        RuleFor(x => x.Serials).Must((x, serials) => x.ManageItem == "S" ? serials?.Count > 0 : true)
            .WithMessage("Serial is Require")
            .Must((x, serials) => x.ManageItem == "S" ? serials?.Sum(b => b.Qty) == x.Qty : true)
            .WithMessage("Serial is not bigger than Qty")
            .Must((x, serials) => x.ManageItem != "S" || serials!.GroupBy(s => s.SerialCode).All(g => g.Count() == 1))
            .WithMessage("Duplicate serial numbers are not allowed.")
            .When(x => x.ManageItem == "S" && x.Serials != null)
            .ForEach(rule => rule.ChildRules(item =>
            {
                item.RuleFor(i => i.SerialCode).NotEmpty().WithMessage("SerialCode must not be empty");
                item.RuleFor(i => i.Qty).GreaterThan(0).WithMessage("Qty should bigger than 0");
            }));
        RuleFor(x => x.Batches).Must((x, batches) => x.ManageItem == "B" ? batches?.Count > 0 : true)
            .WithMessage("Batch is Require")
            .Must((x, batches) => x.ManageItem == "B" ? batches?.Sum(b => b.Qty) == x.Qty : true)
            .WithMessage("Batch Qty must be equal to Qty")
            .ForEach(rule => rule.ChildRules(item =>
            {
                item.RuleFor(i => i.BatchCode).NotEmpty().WithMessage("Batch Number must not be empty");
                item.RuleFor(i => i.Qty).GreaterThan(0).WithMessage("Qty should bigger than 0");
            }));
    }
}
