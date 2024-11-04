namespace Piyavate_Hospital.Shared.Models.Gets;

public record GetListData(
    string DocEntry,
    string DocumentNumber,
    string DocDate,
    string VendorCode,
    string Remarks,
    string TaxDate
    );