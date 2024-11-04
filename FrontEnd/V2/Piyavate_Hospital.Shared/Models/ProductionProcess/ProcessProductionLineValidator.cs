using FluentValidation;

namespace Piyavate_Hospital.Shared.Models.ProductionProcess;

public class ProcessProductionLineValidator : AbstractValidator<ProcessProductionLine>
{
    public ProcessProductionLineValidator()
    {
        RuleFor(x => x.ProductionNo).NotEqual(0).WithMessage("Production No is Require");
        RuleFor(x => x.ProcessStage).NotEmpty().WithMessage("Process Stage is Require");
        RuleFor(x => x.Status).NotEmpty().WithMessage("Status is Require");
        RuleFor(x => x).Custom((x, context) =>
        {
            if (context.RootContextData.TryGetValue("ExistingSerialNumbers", out var existingSerialNumbersObj) &&
                existingSerialNumbersObj is HashSet<int> existingSerialNumbers &&
                existingSerialNumbers.Contains(x.ProductionNo))
            {
                context.AddFailure("ProductionNo", "Duplicate Production No found");
            }
        });
    }
}