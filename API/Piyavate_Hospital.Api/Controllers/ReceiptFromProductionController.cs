using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Piyavate_Hospital.Application.ProcessProduction;
using Piyavate_Hospital.Application.ReceiptFinishGood;
using Piyavate_Hospital.Application.ReturnComponent;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.API.Controllers;

[Route("/receiptFromProduction")]
public class ReceiptFromProductionController(
    ISender mediator,
    IValidator<AddReturnComponentCommand> validator,
    IValidator<ProcessProductionCommand> validatorProcessProduction,
    IValidator<ReceiptFinishGoodCommand> validatorReceiptFinishGood)
    : ApiController
{
    [HttpPost("returnComponent")]
    public async Task<IActionResult> Create(AddReturnComponentCommand command)
    {
        var validationResult = await validator.ValidateAsync(command).ConfigureAwait(false);

        if (!validationResult.IsValid)
        {
            return BadRequest(new PostResponse(
                ErrorMsg: validationResult.Errors[0].ErrorMessage,
                ErrorCode: StatusCodes.Status400BadRequest.ToString()));
        }

        var getData = await mediator.Send(command);
        return getData.Match<IActionResult>(
            data => Ok(data),
            err => BadRequest(new PostResponse
            {
                ErrorCode = err[0].Code,
                ErrorMsg = err[0].Description
            }));
    }

    [HttpPost("updateProcessProduction")]
    public async Task<IActionResult> Create(ProcessProductionCommand command)
    {
        var validationResult = await validatorProcessProduction.ValidateAsync(command).ConfigureAwait(false);

        if (!validationResult.IsValid)
        {
            return BadRequest(new PostResponse(
                ErrorMsg: validationResult.Errors[0].ErrorMessage,
                ErrorCode: StatusCodes.Status400BadRequest.ToString()));
        }

        var getData = await mediator.Send(command);
        return getData.Match<IActionResult>(
            data => Ok(data),
            err => BadRequest(new PostResponse
            {
                ErrorCode = err[0].Code,
                ErrorMsg = err[0].Description
            }));
    }
    [HttpPost("receiptFinishGood")]
    public async Task<IActionResult> Create(ReceiptFinishGoodCommand command)
    {
        var validationResult = await validatorReceiptFinishGood.ValidateAsync(command).ConfigureAwait(false);

        if (!validationResult.IsValid)
        {
            return BadRequest(new PostResponse(
                ErrorMsg: validationResult.Errors[0].ErrorMessage,
                ErrorCode: StatusCodes.Status400BadRequest.ToString()));
        }

        var getData = await mediator.Send(command);
        return getData.Match<IActionResult>(
            data => Ok(data),
            err => BadRequest(new PostResponse
            {
                ErrorCode = err[0].Code,
                ErrorMsg = err[0].Description
            }));
    }
}