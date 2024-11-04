using MediatR;
using ErrorOr;
using Piyavate_Hospital.Application.Common.ErrorHandling;
using Piyavate_Hospital.Application.Common.Interfaces;
using SAPbobsCOM;
using Throw;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.ReceiptFinishGood;

public class ReceiptFinishGoodCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ReceiptFinishGoodCommand, ErrorOr<PostResponse>>
{
    public Task<ErrorOr<PostResponse>> Handle(ReceiptFinishGoodCommand request, CancellationToken cancellationToken)
    {
        var oCompany = unitOfWork.Connect();
        return ErrorHandlingHelper.ExecuteWithHandlingAsync(() =>
        {
            oCompany.ThrowIfNull("Company is null");
            unitOfWork.BeginTransaction(oCompany);
            var oInventoryGenEntry =
                (Documents)oCompany.GetBusinessObject((request.IsDraft)
                    ? BoObjectTypes.oDrafts
                    : BoObjectTypes.oInventoryGenEntry);
            if (!request.IsDraft)
                oInventoryGenEntry.DocObjectCode = BoObjectTypes.oInventoryGenEntry; //oInventoryGenExit
            oInventoryGenEntry.Series = request.Series;
            oInventoryGenEntry.Reference2 = request.Remarks;
            oInventoryGenEntry.DocDate = request.DocDate;
            oInventoryGenEntry.UserFields.Fields.Item("U_WEBID").Value = Guid.NewGuid().ToString();
            foreach (var l in request.Lines)
            {
                oInventoryGenEntry.Lines.BaseEntry = l.DocNum;
                oInventoryGenEntry.Lines.BaseType = 202;
                oInventoryGenEntry.Lines.TransactionType = (BoTransactionTypeEnum)l.TransactionType;
                oInventoryGenEntry.Lines.Quantity = l.Qty;
                switch (l)
                {
                    case { ManageItem: "S", Serials: not null }:
                    {
                        foreach (var serial in l.Serials)
                        {
                            oInventoryGenEntry.Lines.SerialNumbers.InternalSerialNumber = serial.SerialCode;
                            oInventoryGenEntry.Lines.SerialNumbers.Quantity = serial.Qty;
                            oInventoryGenEntry.Lines.SerialNumbers.ManufacturerSerialNumber = serial.MfrNo;
                            oInventoryGenEntry.Lines.SerialNumbers.ManufactureDate = Convert.ToDateTime(serial.MfrDate);
                            oInventoryGenEntry.Lines.SerialNumbers.ExpiryDate = Convert.ToDateTime(serial.ExpDate);
                            oInventoryGenEntry.Lines.SerialNumbers.Add();
                        }

                        break;
                    }
                    case { ManageItem: "B", Batches: not null }:
                    {
                        foreach (var batch in l.Batches)
                        {
                            oInventoryGenEntry.Lines.BatchNumbers.AddmisionDate = DateTime.Now;
                            oInventoryGenEntry.Lines.BatchNumbers.BatchNumber = batch.BatchCode;
                            oInventoryGenEntry.Lines.BatchNumbers.Quantity = batch.Qty;
                            oInventoryGenEntry.Lines.BatchNumbers.ExpiryDate = (DateTime)batch.ExpDate!;
                            oInventoryGenEntry.Lines.BatchNumbers.ManufacturingDate =
                                Convert.ToDateTime(batch.ManfectureDate);
                            oInventoryGenEntry.Lines.BatchNumbers.InternalSerialNumber = batch.LotNo;
                            oInventoryGenEntry.Lines.BatchNumbers.Add();
                        }

                        break;
                    }
                }

                oInventoryGenEntry.Lines.Add();
            }

            if (oInventoryGenEntry.Add() != 0)
            {
                unitOfWork.Rollback(oCompany);
                return Task.FromResult(new PostResponse(
                    oCompany.GetLastErrorCode().ToString(),
                    oCompany.GetLastErrorDescription()).ToErrorOr());
            }

            unitOfWork.Commit(oCompany);
            return Task.FromResult(new PostResponse("", "", "", "", oCompany.GetNewObjectKey()).ToErrorOr());
        }, unitOfWork, oCompany);
    }
}