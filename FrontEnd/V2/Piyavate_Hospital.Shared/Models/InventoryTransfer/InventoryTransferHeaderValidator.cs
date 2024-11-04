

using FluentValidation;

namespace Piyavate_Hospital.Shared.Models.InventoryTransfer;

public class InventoryTransferHeaderValidator : AbstractValidator<InventoryTransferHeader>
{
    public InventoryTransferHeaderValidator()
    {
        RuleFor(x => x.FromWarehouse).NotEmpty().WithMessage("FromWarehouse is require");
        RuleFor(x => x.ToWarehouse).NotEmpty().WithMessage("ToWarehouse is require");
        RuleFor(x => x.Series).NotEmpty().WithMessage("Series is require");
        RuleFor(x => x.DocDate).NotEmpty().WithMessage("DocDate is require");
        RuleFor(x => x.TaxDate).NotEmpty().WithMessage("TaxDate is require");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Lines is require")
            .ForEach(rule => rule.SetValidator(new InventoryTransferLineValidator()));
    }
}

