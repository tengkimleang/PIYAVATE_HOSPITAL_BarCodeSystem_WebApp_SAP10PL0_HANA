using ErrorOr;
using MediatR;
using Piyavate_Hospital.Application.Common.ErrorHandling;
using Piyavate_Hospital.Application.Common.Interfaces;
using SAPbobsCOM;
using Throw;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.InventoryCounting;

public class AddInventoryCountingCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<AddInventoryCountingCommand, ErrorOr<PostResponse>>
{
    public Task<ErrorOr<PostResponse>> Handle(AddInventoryCountingCommand request, CancellationToken cancellationToken)
    {
        var oCompany = unitOfWork.Connect();
        return ErrorHandlingHelper.ExecuteWithHandlingAsync(() =>
        {
            oCompany.ThrowIfNull("Company is null");
            unitOfWork.BeginTransaction(oCompany);
            var oCompanyService = oCompany.GetCompanyService();
            var oInventoryCountingService =
                (InventoryCountingsService)oCompanyService.GetBusinessService(ServiceTypes.InventoryCountingsService);
            var oInventoryCountingParams =
                (InventoryCountingParams)oInventoryCountingService.GetDataInterface(
                    InventoryCountingsServiceDataInterfaces.icsInventoryCountingParams);
            oInventoryCountingParams.DocumentEntry = request.DocEntry;
            var oInventoryCounting = oInventoryCountingService.Get(oInventoryCountingParams);
            oInventoryCounting.Reference2 = request.Ref2;
            oInventoryCounting.Remarks = request.OtherRemark;
            oInventoryCounting.UserFields.Item("U_WEBID").Value = Guid.NewGuid().ToString();
            var oInventoryCountingLines = oInventoryCounting.InventoryCountingLines;
            if (request.InventoryCountingType == "Multiple Count")  
            {
                for (var k = 0; oInventoryCountingLines.Count > k; k++)
                {
                    var oInventoryCountingLine = oInventoryCountingLines.Item(k);
                    foreach (var line in request.Lines)
                    {
                        if (request.CounterId == oInventoryCountingLine.CounterID && line.ItemCode == oInventoryCountingLine.ItemCode)
                        {
                            oInventoryCountingLine.CountedQuantity = line.QtyCounted;
                            if (line.ManageItem.Contains("S") == true)
                            {
                                if (oInventoryCountingLine.InventoryCountingSerialNumbers.Count > 0)
                                    for (var i = 0; oInventoryCountingLine.InventoryCountingSerialNumbers.Count >= i; i++)
                                        oInventoryCountingLine.InventoryCountingSerialNumbers.Remove(0);
                                foreach (var serial in line.Serials)
                                {
                                    if (serial.ConditionSerial == TypeSerial.NEW)
                                    {
                                        InventoryCountingSerialNumber oInventoryCountingSerialNumber =
                                            oInventoryCountingLine.InventoryCountingSerialNumbers.Add();
                                        oInventoryCountingSerialNumber.InternalSerialNumber = serial.SerialCode;
                                        oInventoryCountingSerialNumber.ManufacturerSerialNumber = serial.MfrNo;
                                        oInventoryCountingSerialNumber.ExpiryDate = Convert.ToDateTime(serial.ExpDate);
                                        oInventoryCountingSerialNumber.ManufactureDate =
                                            Convert.ToDateTime(serial.MfrDate);
                                        oInventoryCountingSerialNumber.Location = serial.Location;
                                        oInventoryCountingSerialNumber.ReceptionDate =
                                            Convert.ToDateTime(serial.ReceiptDate);
                                    }
                                    else if (serial.ConditionSerial == TypeSerial.OLD)
                                    {
                                        InventoryCountingSerialNumber oInventoryCountingSerialNumber =
                                            oInventoryCountingLine.InventoryCountingSerialNumbers.Add();
                                        oInventoryCountingSerialNumber.InternalSerialNumber = serial.SerialCode;
                                    }
                                }
                            }
                            else if (line.ManageItem.Contains("B") == true)
                            {
                                for (var i = 0; oInventoryCountingLine.InventoryCountingBatchNumbers.Count >= i; i++)
                                    oInventoryCountingLine.InventoryCountingBatchNumbers.Remove(0);
                                foreach (var batch in line.Batches)
                                {
                                    if (batch.ItemCode == line.ItemCode && batch.BinEntry == line.BinEntry &&
                                        !string.IsNullOrEmpty(batch.BatchCode))
                                    {
                                        if (batch.ConditionBatch == TypeSerial.NEW)
                                        {
                                            InventoryCountingBatchNumber oInventoryCountingBatchNumber =
                                                oInventoryCountingLine.InventoryCountingBatchNumbers.Add();
                                            oInventoryCountingBatchNumber.BatchNumber = batch.BatchCode;
                                            oInventoryCountingBatchNumber.InternalSerialNumber = batch.BatchCode;
                                            oInventoryCountingBatchNumber.Quantity = batch.Qty;
                                        }
                                        else if (batch.ConditionBatch == TypeSerial.OLD)
                                        {
                                            InventoryCountingBatchNumber oInventoryCountingBatchNumber =
                                                oInventoryCountingLine.InventoryCountingBatchNumbers.Add();
                                            oInventoryCountingBatchNumber.BatchNumber = batch.BatchCode;
                                            oInventoryCountingBatchNumber.InternalSerialNumber = batch.BatchCode;
                                            oInventoryCountingBatchNumber.Quantity = line.QtyCounted;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (request.InventoryCountingType == "Single Count")
            {
                foreach (var line in request.Lines)
                {
                    var oInventoryCountingLine = oInventoryCountingLines.Item(line.LineNum);
                    oInventoryCountingLine.CountedQuantity = line.QtyCounted;
                    if (line.ManageItem.Contains("S") == true)
                    {
                        if (oInventoryCountingLine.InventoryCountingSerialNumbers.Count > 0)
                            for (var i = 0; oInventoryCountingLine.InventoryCountingSerialNumbers.Count >= i; i++)
                                oInventoryCountingLine.InventoryCountingSerialNumbers.Remove(0);
                        foreach (var serial in line.Serials)
                        {
                            if (serial.ConditionSerial == TypeSerial.NEW)
                            {
                                InventoryCountingSerialNumber oInventoryCountingSerialNumber =
                                    oInventoryCountingLine.InventoryCountingSerialNumbers.Add();
                                oInventoryCountingSerialNumber.InternalSerialNumber = serial.SerialCode;
                                oInventoryCountingSerialNumber.ManufacturerSerialNumber = serial.MfrNo;
                                oInventoryCountingSerialNumber.ExpiryDate = Convert.ToDateTime(serial.ExpDate);
                                oInventoryCountingSerialNumber.ManufactureDate =
                                    Convert.ToDateTime(serial.MfrDate);
                                oInventoryCountingSerialNumber.Location = serial.Location;
                                oInventoryCountingSerialNumber.ReceptionDate =
                                    Convert.ToDateTime(serial.ReceiptDate);
                            }
                            else if (serial.ConditionSerial == TypeSerial.OLD)
                            {
                                InventoryCountingSerialNumber oInventoryCountingSerialNumber =
                                    oInventoryCountingLine.InventoryCountingSerialNumbers.Add();
                                oInventoryCountingSerialNumber.InternalSerialNumber = serial.SerialCode;
                            }
                        }
                    }
                    else if (line.ManageItem.Contains("B") == true)
                    {
                        if (oInventoryCountingLine.InventoryCountingBatchNumbers.Count > 0)
                            for (var i = 0; oInventoryCountingLine.InventoryCountingBatchNumbers.Count >= i; i++)
                                oInventoryCountingLine.InventoryCountingBatchNumbers.Remove(0);
                        foreach (var batch in line.Batches)
                        {
                            if (batch.ItemCode == line.ItemCode && batch.BinEntry == line.BinEntry &&
                                !string.IsNullOrEmpty(batch.BatchCode))
                            {
                                if (batch.ConditionBatch == TypeSerial.NEW)
                                {
                                    InventoryCountingBatchNumber oInventoryCountingBatchNumber =
                                        oInventoryCountingLine.InventoryCountingBatchNumbers.Add();
                                    oInventoryCountingBatchNumber.BatchNumber = batch.BatchCode;
                                    oInventoryCountingBatchNumber.InternalSerialNumber = batch.BatchCode;
                                    oInventoryCountingBatchNumber.Quantity = batch.Qty;
                                }
                                else if (batch.ConditionBatch == TypeSerial.OLD)
                                {
                                    InventoryCountingBatchNumber oInventoryCountingBatchNumber =
                                        oInventoryCountingLine.InventoryCountingBatchNumbers.Add();
                                    oInventoryCountingBatchNumber.BatchNumber = batch.BatchCode;
                                    oInventoryCountingBatchNumber.InternalSerialNumber = batch.BatchCode;
                                    oInventoryCountingBatchNumber.Quantity = line.QtyCounted;
                                }
                            }
                        }
                    }
                }
            }

            oInventoryCountingService.Update(oInventoryCounting);
            if (oCompany.GetLastErrorCode() != 0)
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
            return Task.FromResult(new PostResponse(
                "",
                "",
                "",
                "",
                oInventoryCounting.DocumentEntry.ToString()).ToErrorOr());
        }, unitOfWork, oCompany);
    }
}