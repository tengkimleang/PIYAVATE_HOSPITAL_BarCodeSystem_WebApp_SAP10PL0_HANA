using ErrorOr;
using MediatR;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.InventoryCounting;

public record AddInventoryCountingCommand(
    int DocEntry,
    TimeSpan CreateTime,
    DateTime CreateDate,
    string OtherRemark,
    string Ref2,
    string InventoryCountingType,
    int CounterId,
    List<InventoryCountingLine> Lines,
    bool IsDraft = false
) : IRequest<ErrorOr<PostResponse>>;

public record InventoryCountingLine(
    string ItemCode,
    // double Qty,
    double QtyCounted,
    string WhsCode,
    int LineNum,
    string ManageItem,
    int CountId,
    int BinEntry,
    string UoM,
    List<InventoryCountingBatch> Batches,
    List<InventoryCountingSerial> Serials);

public record InventoryCountingBatch(
    string ItemCode,
    TypeSerial ConditionBatch,
    string BatchCode,
    double Qty,
    DateTime? ExpireDate,
    DateTime? ManfectureDate,
    DateTime? AdmissionDate,
    string LotNo,
    int BinEntry);

public record InventoryCountingSerial(
    string ItemCode,
    TypeSerial ConditionSerial,
    string SerialCode,
    int SystemSerialNumber,
    string MfrNo,
    DateTime? MfrDate,
    DateTime? ExpDate,
    string Location,
    DateTime? ReceiptDate,
    int BinEntry);
    
    
public enum TypeSerial
{
    NEW,
    OLD
}