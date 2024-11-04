using FluentValidation;

namespace Piyavate_Hospital.Shared.Models.InventoryTransfer;

public class BatchInventoryTransferValidator : AbstractValidator<BatchInventoryTransfer>
{
    public BatchInventoryTransferValidator()
    {
        RuleFor(x => x.Qty).NotEmpty().WithMessage("Item Code is Require");
        RuleFor(x => x.BatchCode).NotEmpty().WithMessage("Batch Code is Require");
        RuleFor(x=>x).Custom((x,context)=>
        {
            if (x.QtyAvailable < x.Qty)
            {
                context.AddFailure("Qty","Qty is not available");
            }
        });
        
    }
}