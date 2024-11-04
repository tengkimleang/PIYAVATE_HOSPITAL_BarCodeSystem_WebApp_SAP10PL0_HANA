using ErrorOr;
using MediatR;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.DeliveryOrder;

public record AddDeliveryOrderCommand(
    string CustomerCode,
    int ContactPersonCode,
    string NumAtCard,
    int Series,
    DateTime DocDate,
    DateTime TaxDate,
    string Remarks,
    List<DeliveryItemLine> Lines,
    bool IsDraft = false
) : IRequest<ErrorOr<PostResponse>>;
public record DeliveryItemLine(
    string ItemCode,
    double Qty,
    double Price,
    string VatCode,
    string WarehouseCode,
    string ManageItem,
    int BaseEntry,
    int BaseLine,
    List<DeliveryBatch>? Batches,
    List<DeliverySerial>? Serials);
public record DeliveryBatch(
    string BatchCode,
    double Qty);
public record DeliverySerial(
    string SerialCode,
    double Qty);