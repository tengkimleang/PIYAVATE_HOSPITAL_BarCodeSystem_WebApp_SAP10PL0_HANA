

using Piyavate_Hospital.Shared.Models.Gets;

namespace Piyavate_Hospital.Shared.Models.InventoryTransfer;

public class InventoryTransferLine
{
    public int LineNum { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public double Qty{ get; set; }
    public string ManageItem { get; set; } = string.Empty;
    public int BaseEntry{get; set; }
    public int BaseLine{get; set; }
    public List<BatchInventoryTransfer>? Batches{get; set; }
    public List<SerialInventoryTransfer>? Serials { get; set; }
}

public class BatchInventoryTransfer
{
    public string BatchCode { get; set; } = string.Empty;
    public double Qty { get; set; }
    public DateTime? ExpDate { get; set; } = DateTime.Today;
    public DateTime? ManfectureDate { get; set; } = DateTime.Today;
    public DateTime? AdmissionDate { get; set; } = DateTime.Today;
    public double QtyAvailable { get; set; }
    public string LotNo { get; set; } = string.Empty;
    public IEnumerable<GetBatchOrSerial> OnSelectedBatchOrSerial { get; set; } = Array.Empty<GetBatchOrSerial>();
}

public class SerialInventoryTransfer
{
    public string SerialCode { get; set; } = string.Empty;
    public int Qty { get; set; } = 1;
    public string MfrNo { get; set; } = string.Empty;
    public DateTime? MfrDate { get; set; } = DateTime.Today;
    public DateTime? ExpDate { get; set; } = DateTime.Today;
    public IEnumerable<GetBatchOrSerial> OnSelectedBatchOrSerial { get; set; } = Array.Empty<GetBatchOrSerial>();
}
