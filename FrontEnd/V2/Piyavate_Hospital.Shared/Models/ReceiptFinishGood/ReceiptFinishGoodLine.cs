using Piyavate_Hospital.Shared.Models.Gets;

namespace Piyavate_Hospital.Shared.Models.ReceiptFinishGood;

public class ReceiptFinishGoodLine
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int DocNum { get; set; }
    public int LineNum { get; set; }
    public int BaseLineNum { get; set; }
    public double Qty { get; set; }
    public double QtyPlan { get; set; }
    public string UomName { get; set; } = string.Empty;
    public string WhsCode { get; set; } = string.Empty;
    public string ManageItem { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public List<ReceiptFinishGoodBatch> Batches { get; set; } = new();
    public List<ReceiptFinishGoodSerial> Serials { get; set; } = new();
}

public class ReceiptFinishGoodBatch
{
    public string BatchCode { get; set; } = string.Empty;
    public double Qty { get; set; }
    public double QtyAvailable { get; set; }
    public DateTime? ExpDate { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public string LotNo { get; set; } = string.Empty;
    public IEnumerable<GetBatchOrSerial> OnSelectedBatchOrSerial { get; set; } = Array.Empty<GetBatchOrSerial>();
}

public class ReceiptFinishGoodSerial
{
    public string SerialCode { get; set; } = string.Empty;
    public double Qty { get; set; }
    public string MfrNo { get; set; } = string.Empty;
    public DateTime? MfrDate { get; set; }
    public DateTime? ExpDate { get; set; }
    public IEnumerable<GetBatchOrSerial> OnSelectedBatchOrSerial { get; set; } = Array.Empty<GetBatchOrSerial>();
}