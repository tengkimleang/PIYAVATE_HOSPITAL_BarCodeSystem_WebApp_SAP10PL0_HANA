using ErrorOr;
using MediatR;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.IssueForProductions;

public record AddIssueForProductionCommand(
    int Series,
    DateTime DocDate,
    string Remarks,
    List<IssueForProductionLine> Lines,
    bool IsDraft = false
) : IRequest<ErrorOr<PostResponse>>;

public record IssueForProductionLine(
    int DocNum,
    int BaseLineNum,
    double Qty,
    string WhsCode,
    string ManageItem,
    List<Batch>? Batches,
    List<Serial>? Serials
);

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