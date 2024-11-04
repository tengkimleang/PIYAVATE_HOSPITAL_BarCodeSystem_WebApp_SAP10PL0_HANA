

using Piyavate_Hospital.Shared.Models.Gets;

namespace Piyavate_Hospital.Shared.Models.DeliveryOrder;

public class DeliveryOrderLine
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
    public List<BatchDeliveryOrder>? Batches { get; set; }
    public List<SerialDeliveryOrder>? Serials { get; set; }
}

public class BatchDeliveryOrder
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

public class SerialDeliveryOrder
{
    public string SerialCode { get; set; } = string.Empty;
    public int Qty { get; set; } = 1;
    public string MfrNo { get; set; } = string.Empty;
    public DateTime? MfrDate { get; set; } = DateTime.Today;
    public DateTime? ExpDate { get; set; } = DateTime.Today;
    public IEnumerable<GetBatchOrSerial> OnSelectedBatchOrSerial { get; set; } = Array.Empty<GetBatchOrSerial>();
}
