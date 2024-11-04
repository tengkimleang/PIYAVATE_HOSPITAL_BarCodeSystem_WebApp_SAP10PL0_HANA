using FluentValidation;

namespace Piyavate_Hospital.Application.ProcessProduction;

public class ProcessProductionCommandValidator : AbstractValidator<ProcessProductionCommand>
{
    public ProcessProductionCommandValidator()
    {
        RuleFor(x => x.Data).NotNull().WithMessage("ProcessProduction is required")
            .ForEach(rule => rule.ChildRules(item =>
            {
                item.RuleFor(i => i.ProductionNo).NotEqual(0).WithMessage("ProductionNo must not be empty");
                item.RuleFor(i => i.ProcessStage).NotEmpty().WithMessage("ProcessStage must not be empty");
                item.RuleFor(i => i.Status).NotEmpty().WithMessage("Status must not be empty");
            }));
    }
}