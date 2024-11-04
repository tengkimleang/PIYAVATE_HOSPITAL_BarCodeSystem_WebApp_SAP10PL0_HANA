

using ErrorOr;
using MediatR;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.InventoryTransfer;

public record AddInventoryTransferCommand(
    int Series,
    DateTime DocDate,
    DateTime TaxDate,
    string Remarks,
    string FromWarehouse,
    string ToWarehouse,
    List<InventoryTransferLine> Lines,
    bool IsDraft = false
    ) : IRequest<ErrorOr<PostResponse>>;

public record InventoryTransferLine(
    string ItemCode,
    double Qty,
    string ManageItem,
    int BaseEntry,
    int BaseLine,
    List<Batch>? Batches,
    List<Serial>? Serials);

public record Batch(
    string BatchCode,
    double Qty,
    DateTime? ExpDate,
    DateTime? ManfectureDate,
    DateTime? AdmissionDate,
    string LotNo);

public record Serial(
    string SerialCode,
    double Qty,
    string MfrNo,
    DateTime? MfrDate,
    DateTime? ExpDate);