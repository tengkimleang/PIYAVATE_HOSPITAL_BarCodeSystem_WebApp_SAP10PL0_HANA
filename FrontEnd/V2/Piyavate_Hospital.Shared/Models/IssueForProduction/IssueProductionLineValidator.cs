﻿using FluentValidation;

namespace Piyavate_Hospital.Shared.Models.IssueForProduction;

public class IssueProductionLineValidator : AbstractValidator<IssueProductionLine>
{
    public IssueProductionLineValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().WithMessage("Item Code is Require");
        RuleFor(x => x.Qty).NotNull().WithMessage("Qty is Require");
        RuleFor(x => x.Qty).GreaterThan(0).WithMessage("Qty is Require");
        RuleFor(x => x.Serials).Must((x, serials) => x.ManageItem == "S" ? serials?.Count > 0 : true)
            .WithMessage("Serial is Require")
            .Must((x, serials) => x.ManageItem != "S" || serials!.GroupBy(s => s.SerialCode).All(g => g.Count() == 1))
            .WithMessage("Duplicate serial numbers are not allowed.")
            .When(x => x.ManageItem == "S" && x.Serials != null)
            .ForEach(rule => rule.ChildRules(item =>
            {
                item.RuleFor(i => i.SerialCode).NotEmpty().WithMessage("SerialCode must not be empty");
                item.RuleFor(i => i.Qty).GreaterThan(0).WithMessage("Qty should bigger than 0");
            }));
        RuleFor(x => x.Batches).Must((x, batches)=> x.ManageItem=="B"? batches?.Count > 0 : true)
            .WithMessage("Batch is Require")
            .ForEach(rule => rule.ChildRules(item =>
            {
                item.RuleFor(i => i.BatchCode).NotEmpty().WithMessage("Batch Number must not be empty");
                //todo: check if this is correct
                //item.RuleFor(i => i.Qty).GreaterThan(0).WithMessage("Qty should bigger than 0");
            }));
    }
}