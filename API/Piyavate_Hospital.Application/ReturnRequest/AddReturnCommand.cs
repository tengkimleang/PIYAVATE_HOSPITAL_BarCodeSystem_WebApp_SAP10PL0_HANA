
using ErrorOr;
using MediatR;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.ReturnRequest;

public record AddReturnRequestCommand(
    string CustomerCode,
    int Series,
    DateTime DocDate,
    DateTime TaxDate,
    List<ReturnRequestLine> Lines,
    string Remarks="",
    string NumAtCard ="",
    int ContactPersonCode=0,
    bool IsDraft=false) : IRequest<ErrorOr<PostResponse>>;

public record ReturnRequestLine(
    string ItemCode,
    double Qty,
    double Price,
    string VatCode,
    string WarehouseCode,
    string ManageItem,
    int BaseEntry,
    int BaseLine,
    List<BatchReturnRequest> Batches,
    List<SerialReturnRequest> Serials);

public record BatchReturnRequest(
    string BatchCode,
    double Qty,
    DateTime ExpireDate,
    DateTime ManfectureDate,
    DateTime AdmissionDate,
    string LotNo);

public record SerialReturnRequest(
    string SerialCode,
    double Qty,
    string MfrNo,
    DateTime MfrDate,
    DateTime ExpDate);