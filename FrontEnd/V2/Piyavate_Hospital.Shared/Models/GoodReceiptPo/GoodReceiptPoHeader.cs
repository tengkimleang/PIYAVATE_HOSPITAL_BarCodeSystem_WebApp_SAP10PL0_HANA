
using System.ComponentModel.DataAnnotations;

namespace Piyavate_Hospital.Shared.Models.GoodReceiptPo;

public class GoodReceiptPoHeader
{
    public string VendorCode { get; set; } = string.Empty;
    public string ContactPersonCode { get; set; } = "0";
    public string VendorNo { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public DateTime? DocDate { get; set; }
    public DateTime? TaxDate { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public bool IsDraft { get; set; } = false;
    public List<GoodReceiptPoLine>? Lines { get; set; }
}
