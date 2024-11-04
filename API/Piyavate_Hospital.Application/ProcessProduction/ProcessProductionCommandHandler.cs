using ErrorOr;
using MediatR;
using Piyavate_Hospital.Application.Common.ErrorHandling;
using Piyavate_Hospital.Application.Common.Interfaces;
using SAPbobsCOM;
using Piyavate_Hospital.Domain.Common;
using Throw;

namespace Piyavate_Hospital.Application.ProcessProduction;

public class ProcessProductionCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ProcessProductionCommand, ErrorOr<PostResponse>>
{
    public Task<ErrorOr<PostResponse>> Handle(ProcessProductionCommand request, CancellationToken cancellationToken)
    {
        var oCompany = unitOfWork.Connect();
        return ErrorHandlingHelper.ExecuteWithHandlingAsync(() =>
        {
            oCompany.ThrowIfNull("Company is null");
            unitOfWork.BeginTransaction(oCompany);
            foreach (var obj in request.Data)
            {
                var oProductionOrders = (ProductionOrders)oCompany.GetBusinessObject(BoObjectTypes.oProductionOrders);
                if (oProductionOrders.GetByKey(obj.ProductionNo))
                {
                    oProductionOrders.UserFields.Fields.Item("U_Status").Value = obj.Status;
                    oProductionOrders.UserFields.Fields.Item("U_ProcessStage").Value = obj.ProcessStage;
                    if (oProductionOrders.Update() != 0)
                    {
                        unitOfWork.Rollback(oCompany);
                        return Task.FromResult(new PostResponse(
                            oCompany.GetLastErrorCode().ToString(),
                            oCompany.GetLastErrorDescription(),
                            "",
                            "",
                            "").ToErrorOr());
                    }
                }
            }

            unitOfWork.Commit(oCompany);
            return Task.FromResult(new PostResponse("", "", "", "", oCompany.GetNewObjectKey()).ToErrorOr());
        }, unitOfWork, oCompany);
    }
}