
namespace Piyavate_Hospital.Shared.Models.Gets;

public record GoodReceiptPoHeaderDeatialByDocNum(
    string SeriesName,
    string DocNum,
    string DocDate,
    string TaxDate,
    string Vendor,
    string ContactPerson,
    string RefInv,
    string WhsFrom,
    string WhsTo,
    string DocEntry
    );
