
using ErrorOr;
using Piyavate_Hospital.Application.Common.Interfaces;
using SAPbobsCOM;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.Common.ErrorHandling;

public static class ErrorHandlingHelper
{
    public static async Task<ErrorOr<PostResponse>> ExecuteWithHandlingAsync(Func<Task<ErrorOr<PostResponse>>> action, IUnitOfWork unitOfWork, Company oCompany)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback(oCompany);
            return new PostResponse(
                "Exception",
                ex.Message,
                "",
                "",
                "").ToErrorOr();
        }
    }
}