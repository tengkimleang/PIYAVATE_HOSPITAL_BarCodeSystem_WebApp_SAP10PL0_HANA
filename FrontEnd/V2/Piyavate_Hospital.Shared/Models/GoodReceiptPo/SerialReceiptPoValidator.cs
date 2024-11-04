using FluentValidation;

namespace Piyavate_Hospital.Shared.Models.GoodReceiptPo;

public class SerialReceiptPoValidator : AbstractValidator<SerialReceiptPo>
{
    public SerialReceiptPoValidator()
    {
        RuleFor(x => x.Qty).NotEmpty().WithMessage("Item Code is Require");
        RuleFor(x => x.SerialCode).NotEmpty().WithMessage("Batch Code is Require");
        // RuleFor(x=>x).Custom((x,context)=>
        // {
        //     if (x.QtyAvailable < x.Qty)
        //     {
        //         context.AddFailure("Qty","Qty is not available");
        //     }
        // });
        
    }
}