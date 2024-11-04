

using ErrorOr;
using MediatR;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.GoodReceiptPo;

public record AddGoodReceiptPoCommand(
    string VendorCode,
    int ContactPersonCode,
    string VendorNo,
    int Series,
    DateTime DocDate,
    DateTime TaxDate,
    string Remarks,
    List<GoodReceiptPoLine> Lines,
    bool IsDraft = false
    ) : IRequest<ErrorOr<PostResponse>>;

public record GoodReceiptPoLine(
    string ItemCode,
    double Qty,
    double Price,
    string VatCode,
    string WarehouseCode,
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