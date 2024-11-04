

using ErrorOr;
using MediatR;
using Piyavate_Hospital.Application.Common.ErrorHandling;
using Piyavate_Hospital.Application.Common.Interfaces;
using SAPbobsCOM;
using Throw;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.InventoryTransfer;

public class AddInventoryTransferCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddInventoryTransferCommand, ErrorOr<PostResponse>>
{

    public Task<ErrorOr<PostResponse>> Handle(AddInventoryTransferCommand request, CancellationToken cancellationToken)
    {
        var oCompany = unitOfWork.Connect();
        return ErrorHandlingHelper.ExecuteWithHandlingAsync(() =>
        {
            oCompany.ThrowIfNull("Company is null");
            unitOfWork.BeginTransaction(oCompany);
            var oGoodReceiptPo = (StockTransfer)oCompany.GetBusinessObject((request.IsDraft) ? BoObjectTypes.oDrafts : BoObjectTypes.oStockTransfer);
            if (request.IsDraft) oGoodReceiptPo.DocObjectCode = BoObjectTypes.oStockTransfer;
            oGoodReceiptPo.Series = request.Series;
            oGoodReceiptPo.DocDate = request.DocDate;
            oGoodReceiptPo.TaxDate = request.TaxDate;
            oGoodReceiptPo.Comments = request.Remarks;
            oGoodReceiptPo.FromWarehouse = request.FromWarehouse;
            oGoodReceiptPo.ToWarehouse = request.ToWarehouse;
            oGoodReceiptPo.UserFields.Fields.Item("U_WEBID").Value = Guid.NewGuid().ToString();
            foreach (var l in request.Lines!)
            {
                oGoodReceiptPo.Lines.ItemCode = l.ItemCode;
                oGoodReceiptPo.Lines.Quantity = l.Qty;

                if (l.BaseEntry != 0)
                {
                    oGoodReceiptPo.Lines.BaseEntry = l.BaseEntry;
                    oGoodReceiptPo.Lines.BaseType = InvBaseDocTypeEnum.InventoryTransferRequest;
                    oGoodReceiptPo.Lines.BaseLine = l.BaseLine;
                }
                switch (l)
                {
                    case { ManageItem: "S", Serials: not null }:
                        {
                            foreach (var serial in l.Serials)
                            {
                                oGoodReceiptPo.Lines.SerialNumbers.InternalSerialNumber = serial.SerialCode;
                                oGoodReceiptPo.Lines.SerialNumbers.Quantity = serial.Qty;
                                oGoodReceiptPo.Lines.SerialNumbers.ManufacturerSerialNumber = serial.MfrNo;
                                oGoodReceiptPo.Lines.SerialNumbers.ManufactureDate = Convert.ToDateTime(serial.MfrDate);
                                oGoodReceiptPo.Lines.SerialNumbers.ExpiryDate = Convert.ToDateTime(serial.ExpDate);
                                oGoodReceiptPo.Lines.SerialNumbers.Add();
                            }

                            break;
                        }
                    case { ManageItem: "B", Batches: not null }:
                        {
                            foreach (var batch in l.Batches)
                            {
                                oGoodReceiptPo.Lines.BatchNumbers.AddmisionDate = DateTime.Now;
                                oGoodReceiptPo.Lines.BatchNumbers.BatchNumber = batch.BatchCode;
                                oGoodReceiptPo.Lines.BatchNumbers.Quantity = batch.Qty;
                                oGoodReceiptPo.Lines.BatchNumbers.ExpiryDate = (DateTime)batch.ExpDate!;
                                oGoodReceiptPo.Lines.BatchNumbers.ManufacturingDate = Convert.ToDateTime(batch.ManfectureDate);
                                oGoodReceiptPo.Lines.BatchNumbers.InternalSerialNumber = batch.LotNo;
                                oGoodReceiptPo.Lines.BatchNumbers.Add();
                            }

                            break;
                        }
                }

                oGoodReceiptPo.Lines.Add();
            }
            if (oGoodReceiptPo.Add() != 0)
            {
                unitOfWork.Rollback(oCompany);
                return Task.FromResult(new PostResponse(
                    oCompany.GetLastErrorCode().ToString(),
                    oCompany.GetLastErrorDescription(),
                    "",
                    "",
                    "").ToErrorOr());
            }
            unitOfWork.Commit(oCompany);
            return Task.FromResult(new PostResponse("", "", "", "", oCompany.GetNewObjectKey()).ToErrorOr());
        }, unitOfWork, oCompany);
    }
}