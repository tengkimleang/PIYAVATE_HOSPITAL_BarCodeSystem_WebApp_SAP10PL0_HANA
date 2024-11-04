using ErrorOr;
using MediatR;
using Piyavate_Hospital.Application.Common.ErrorHandling;
using Piyavate_Hospital.Application.Common.Interfaces;
using SAPbobsCOM;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.DeliveryOrder;

public class AddDeliveryOrderCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<AddDeliveryOrderCommand, ErrorOr<PostResponse>>
{
    public Task<ErrorOr<PostResponse>> Handle(AddDeliveryOrderCommand request, CancellationToken cancellationToken)
    {
        var oCompany = unitOfWork.Connect();
        return ErrorHandlingHelper.ExecuteWithHandlingAsync(() =>
        {
            unitOfWork.BeginTransaction(oCompany);
            // var oDeliveryOrder = (Documents)oCompany.GetBusinessObject(BoObjectTypes.oDeliveryNotes);
            var oDeliveryOrder = (Documents)oCompany.GetBusinessObject((request.IsDraft) ? BoObjectTypes.oDrafts : BoObjectTypes.oDeliveryNotes);
            if (request.IsDraft) oDeliveryOrder.DocObjectCode = BoObjectTypes.oDeliveryNotes;
            oDeliveryOrder.CardCode = request.CustomerCode;
            oDeliveryOrder.ContactPersonCode = request.ContactPersonCode;
            oDeliveryOrder.NumAtCard = request.NumAtCard;
            oDeliveryOrder.Series = request.Series;
            oDeliveryOrder.DocDate = request.DocDate;
            oDeliveryOrder.DocDueDate = request.TaxDate;
            oDeliveryOrder.Comments = request.Remarks;
            oDeliveryOrder.UserFields.Fields.Item("U_WEBID").Value = Guid.NewGuid().ToString();
            foreach (var l in request.Lines!)
            {
                oDeliveryOrder.Lines.ItemCode = l.ItemCode;
                oDeliveryOrder.Lines.Quantity = l.Qty;
                oDeliveryOrder.Lines.UnitPrice = l.Price;
                oDeliveryOrder.Lines.VatGroup = l.VatCode;
                oDeliveryOrder.Lines.WarehouseCode = l.WarehouseCode;
                if (l.BaseEntry != 0)
                {
                    oDeliveryOrder.Lines.BaseEntry = Convert.ToInt32(l.BaseEntry);
                    oDeliveryOrder.Lines.BaseType = 17;
                    oDeliveryOrder.Lines.BaseLine = l.BaseLine;
                }

                if (l.ManageItem == "S")
                {
                    foreach (var serial in l.Serials!)
                    {
                        //oDeliveryOrder.Lines.SerialNumbers.SystemSerialNumber = Convert.ToInt32(serial.SerialCode);
                        oDeliveryOrder.Lines.SerialNumbers.InternalSerialNumber = serial.SerialCode;
                        oDeliveryOrder.Lines.SerialNumbers.Add();
                    }
                }
                else if (l.ManageItem == "B")
                {
                    foreach (var batch in l.Batches!)
                    {
                        oDeliveryOrder.Lines.BatchNumbers.BatchNumber = batch.BatchCode;
                        //oDeliveryOrder.Lines.BatchNumbers.InternalSerialNumber = batch.BatchCode;
                        oDeliveryOrder.Lines.BatchNumbers.Quantity = batch.Qty;
                        oDeliveryOrder.Lines.BatchNumbers.Add();
                    }
                }

                oDeliveryOrder.Lines.Add();
            }
            if (oDeliveryOrder.Add() != 0)
            {
                unitOfWork.Rollback(oCompany);
                return Task.FromResult(new PostResponse(oCompany.GetLastErrorCode().ToString(), oCompany.GetLastErrorDescription(), "", "", "").ToErrorOr());
            }
            unitOfWork.Commit(oCompany);
            return Task.FromResult(new PostResponse("", "", "", "", oCompany.GetNewObjectKey()).ToErrorOr());
        }, unitOfWork, oCompany);
    }
}
