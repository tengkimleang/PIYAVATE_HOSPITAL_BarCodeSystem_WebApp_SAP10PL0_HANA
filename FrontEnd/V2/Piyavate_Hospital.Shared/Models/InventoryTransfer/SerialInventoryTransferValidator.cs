using FluentValidation;

namespace Piyavate_Hospital.Shared.Models.InventoryTransfer;

public class SerialInventoryTransferValidator : AbstractValidator<SerialInventoryTransfer>
{
    public SerialInventoryTransferValidator()
    {
        RuleFor(x => x.Qty).NotEmpty().WithMessage("Item Code is Require");
        RuleFor(x => x.SerialCode).NotEmpty().WithMessage("Batch Code is Require");
        RuleFor(x => x).Custom((x, context) =>
        {
            if (context.RootContextData.TryGetValue("ExistingSerialNumbers", out var existingSerialNumbersObj) &&
                existingSerialNumbersObj is HashSet<string> existingSerialNumbers &&
                existingSerialNumbers.Contains(x.SerialCode))
            {
                context.AddFailure("SerialCode", "Duplicate serial number found");
            }
        });
    }
}