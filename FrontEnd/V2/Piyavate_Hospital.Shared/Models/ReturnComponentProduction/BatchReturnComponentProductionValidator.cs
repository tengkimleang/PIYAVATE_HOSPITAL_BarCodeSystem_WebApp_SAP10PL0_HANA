using FluentValidation;

namespace Piyavate_Hospital.Shared.Models.ReturnComponentProduction;

public class BatchReturnComponentProductionValidator : AbstractValidator<BatchReturnComponentProduction>
{
    public BatchReturnComponentProductionValidator()
    {
        RuleFor(x => x).Custom((x, context) =>
        {
            if (x.QtyAvailable < x.Qty)
            {
                context.AddFailure("Qty", "Qty is not available");
            }

            if (x is { Qty: 0, QtyManual: 0 })
            {
                context.AddFailure("Qty", "Either Qty or QtyManual must be provided");
            }
        });
        RuleFor(x => x.BatchCode).NotEmpty().WithMessage("Batch Code is Require");
    }
}