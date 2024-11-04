using FluentValidation;

namespace Piyavate_Hospital.Application.InventoryCounting
{
    public class AddInventoryCountingCommandValidator : AbstractValidator<AddInventoryCountingCommand>
    {
        public AddInventoryCountingCommandValidator()
        {
            RuleFor(x => x.DocEntry).NotEmpty().WithMessage("DocEntry is not Empty");
        }
    }
}
