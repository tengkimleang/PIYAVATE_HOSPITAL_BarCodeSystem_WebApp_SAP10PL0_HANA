using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Piyavate_Hospital.Application.Return;
using Piyavate_Hospital.Application.ReturnRequest;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.API.Controllers;

[Route("/returnRequest")]
public class ReturnRequestController(ISender mediator, IValidator<AddReturnRequestCommand> validator)
    : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Create(AddReturnRequestCommand command)
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