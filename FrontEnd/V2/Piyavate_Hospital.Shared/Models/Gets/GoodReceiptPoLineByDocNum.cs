

namespace Piyavate_Hospital.Shared.Models.Gets;

// public record GoodReceiptPoLineByDocNum(
//    
//     );

public class GoodReceiptPoLineByDocNum
{
    public string BaseLineNumber { get; set; } = string.Empty;
    public string DocEntry { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Qty { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string LineTotal { get; set; } = string.Empty;
    public string VatCode { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string BarCode { get; set; } = string.Empty;
    public string ManageItem { get; set; } = string.Empty;
    public List<BatchGoodReceiptPoCopyFrom> Batches { get; set; } = new();
    public List<SerialGoodReceiptPoCopyFrom> Serials { get; set; }= new();
}

public class BatchGoodReceiptPoCopyFrom
{
    public string BatchCode { get; set; } = string.Empty;
    public double Qty { get; set; }
    public double QtyAvailable { get; set; }
    public DateTime? ExpDate { get; set; } = DateTime.Today;
    public DateTime? ManfectureDate { get; set; } = DateTime.Today;
    public DateTime? AdmissionDate { get; set; } = DateTime.Today;
    public string LotNo { get; set; } = string.Empty;
    public IEnumerable<GetBatchOrSerial> OnSelectedBatchOrSerial { get; set; } = Array.Empty<GetBatchOrSerial>();
}

public class SerialGoodReceiptPoCopyFrom
{
    public string SerialCode { get; set; } = string.Empty;
    public int Qty { get; set; } = 1;
    public string MfrNo { get; set; } = string.Empty;
    public DateTime? MfrDate { get; set; } = DateTime.Today;
    public DateTime? ExpDate { get; set; } = DateTime.Today;
    public IEnumerable<GetBatchOrSerial> OnSelectedBatchOrSerial { get; set; } = Array.Empty<GetBatchOrSerial>();
}
