using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Piyavate_Hospital.Application.ARCreditMemo;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.API.Controllers;


[Route("/arCreditMemo")]
public class ArCreditMemoController(ISender mediator) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Create(AddARCreditMemoCommand command, IValidator<AddARCreditMemoCommand> validator)
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
}
