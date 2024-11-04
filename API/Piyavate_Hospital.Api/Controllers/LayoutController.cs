using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piyavate_Hospital.Application.Layout;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.API.Controllers;

[Route("/layoutEndpoint")]
public class LayoutController(
    ISender mediator,
    IValidator<LayoutCommand> validator,
    IWebHostEnvironment webHostEnvironment)
    : ApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Create(string docEntry, string layoutCode)
    {
        LayoutCommand command = new()
        {
            DocEntry = docEntry,
            LayoutCode = layoutCode,
            StoreName = "_USP_CALLTRANS_EWTRANSACTION",
        };
        var validationResult = await validator.ValidateAsync(command).ConfigureAwait(false);

        if (!validationResult.IsValid)
        {
            return BadRequest(new PostResponse(
                ErrorMsg: validationResult.Errors[0].ErrorMessage,
                ErrorCode: StatusCodes.Status400BadRequest.ToString()));
        }

        command.Path = Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot", "Layouts");

        var getData = await mediator.Send(command);
        return File(getData.Data ?? [], getData.ApplicationType, getData.FileName);
    }
}   