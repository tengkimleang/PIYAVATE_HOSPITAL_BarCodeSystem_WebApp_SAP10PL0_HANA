
using ErrorOr;
using MediatR;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.Return;

public record AddReturnCommand(
    string CustomerCode,
    int Series,
    DateTime DocDate,
    DateTime TaxDate,
    List<ReturnLine> Lines,
    string Remarks="",
    string NumAtCard ="",
    int ContactPersonCode=0,
    bool IsDraft=false) : IRequest<ErrorOr<PostResponse>>;

public record ReturnLine(
    string ItemCode,
    double Qty,
    double Price,
    string VatCode,
    string WarehouseCode,
    string ManageItem,
    int BaseEntry,
    int BaseLine,
    List<BatchReturn> Batches,
    List<SerialReturn> Serials);

public record BatchReturn(
    string BatchCode,
    double Qty,
    DateTime ExpireDate,
    DateTime ManfectureDate,
    DateTime AdmissionDate,
    string LotNo);

public record SerialReturn(
    string SerialCode,
    double Qty,
    string MfrNo,
    DateTime MfrDate,
    DateTime ExpDate);