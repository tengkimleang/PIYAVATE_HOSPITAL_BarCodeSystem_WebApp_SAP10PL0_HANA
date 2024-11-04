using FluentValidation;

namespace Piyavate_Hospital.Application.Layout;

public class LayoutCommandValidator : AbstractValidator<LayoutCommand>
{
    public LayoutCommandValidator()
    {
        RuleFor(x => x.LayoutCode).NotEmpty().WithMessage("LayoutCode Need to Specify");
        RuleFor(x => x.DocEntry).NotEmpty().WithMessage("DocEntry need to Specify");
    }
}
