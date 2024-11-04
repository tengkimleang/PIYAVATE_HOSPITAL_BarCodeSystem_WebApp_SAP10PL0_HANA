using Piyavate_Hospital.Shared.Models.Gets;

namespace Piyavate_Hospital.Shared.Models.InventoryCounting;

public class InventoryCountingLine
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string WhsCode { get; set; } = string.Empty;
    public double Qty { get; set; }
    public double QtyCounted { get; set; }
    public string Counted { get; set; } = string.Empty;
    public int LineNum { get; set; }
    public string ManageItem { get; set; } = String.Empty;
    public int CountId { get; set; }
    public int BinEntry { get; set; }
    public string Uom { get; set; } = string.Empty;
    public List<InventoryCountingBatch> Batches { get; set; } = new();
    public List<InventoryCountingSerial> Serials { get; set; } = new();
}
public enum TypeSerial
{
    NEW,
    OLD
}
public class InventoryCountingBatch
{
    public string ItemCode { get; set; } = string.Empty;
    public TypeSerial ConditionBatch { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public double Qty { get; set; }
    public double QtyAvailable { get; set; }
    public DateTime? ExpireDate { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public string LotNo { get; set; } = string.Empty;
    public int BinEntry { get; set; }
    public string IsBatchNew { get; set; } = string.Empty;
    public IEnumerable<GetBatchOrSerial> OnSelectedBatchOrSerial { get; set; } = Array.Empty<GetBatchOrSerial>();
}

public class InventoryCountingSerial
{
    public string ItemCode { get; set; } = string.Empty;
    public TypeSerial ConditionSerial { get; set; }
    public int Qty { get; set; }
    public string SerialCode { get; set; } = string.Empty;
    public int SystemSerialNumber { get; set; }
    public string MfrNo { get; set; } = string.Empty;
    public DateTime? MfrDate { get; set; }
    public DateTime? ExpDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public int BinEntry { get; set; }
    public string IsSerialNew { get; set; } = string.Empty;
    public IEnumerable<GetBatchOrSerial> OnSelectedBatchOrSerial { get; set; } = Array.Empty<GetBatchOrSerial>();
}