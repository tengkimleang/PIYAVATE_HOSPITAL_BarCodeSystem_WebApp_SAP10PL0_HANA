using FluentValidation;

namespace Piyavate_Hospital.Shared.Models.InventoryCounting;

public class InventoryCountingHeaderValidator : AbstractValidator<InventoryCountingHeader>
{
    public InventoryCountingHeaderValidator()
    {
        RuleFor(x => x.DocEntry).NotEmpty().WithMessage("DocEntry is require");
        RuleFor(x => x.CreateTime).NotEmpty().WithMessage("CreateTime is require");
        RuleFor(x => x.CreateDate).NotEmpty().WithMessage("CreateDate is require");
        // RuleFor(x => x.Ref2).NotEmpty().WithMessage("Ref2 is require");
        // RuleFor(x => x.OtherRemark).NotEmpty().WithMessage("OtherRemark is require");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Lines is require")
            .ForEach(rule => rule.SetValidator(new InventoryCountingLineValidator()));
    }
}