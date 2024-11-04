
namespace Piyavate_Hospital.Shared.Models.GoodReceiptPo;

public class GoodReceiptPoLine
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public double? Qty { get; set; }
    public int LineNum { get; set; }
    public double? Price { get; set; }
    public string? VatCode { get; set; }
    public string? WarehouseCode { get; set; }
    public string? ManageItem { get; set; }
    public int BaseEntry { get; set; }
    public int BaseLine { get; set; }
    public List<BatchReceiptPo>? Batches { get; set; }
    public List<SerialReceiptPo>? Serials { get; set; }
}

public class BatchReceiptPo
{
    public string BatchCode { get; set; } = string.Empty;
    public double Qty { get; set; }
    public DateTime? ExpDate { get; set; } = DateTime.Today;
    public DateTime? ManfectureDate { get; set; } = DateTime.Today;
    public DateTime? AdmissionDate { get; set; } = DateTime.Today;
    public string LotNo { get; set; } = string.Empty;
}

public class SerialReceiptPo
{
    public string SerialCode { get; set; } = string.Empty;
    public int Qty { get; set; } = 1;
    public string MfrNo { get; set; } = string.Empty;
    public DateTime? MfrDate { get; set; } = DateTime.Today;
    public DateTime? ExpDate { get; set; } = DateTime.Today;
}
