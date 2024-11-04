using FluentValidation;

namespace Piyavate_Hospital.Shared.Models.DeliveryOrder;

public class DeliveryOrderHeaderValidator : AbstractValidator<DeliveryOrderHeader>
{
    public DeliveryOrderHeaderValidator()
    {
        RuleFor(x => x.CustomerCode).NotEmpty().WithMessage("Vendor is require");
        RuleFor(x => x.Series).NotEmpty().WithMessage("Series is require");
        RuleFor(x => x.DocDate).NotEmpty().WithMessage("DocDate is require");
        RuleFor(x => x.TaxDate).NotEmpty().WithMessage("TaxDate is require");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Lines is require")
            .ForEach(rule => rule.SetValidator(new DeliveryOrderLineValidator()));
    }
}
