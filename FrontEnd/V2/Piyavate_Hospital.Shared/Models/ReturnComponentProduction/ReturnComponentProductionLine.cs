using Piyavate_Hospital.Shared.Models.Gets;

namespace Piyavate_Hospital.Shared.Models.ReturnComponentProduction;

public class ReturnComponentProductionLine
{
    // private double _qty;
    public string DocNum { get; set; } = string.Empty;
    public int LineNum { get; set; }
    public int BaseLineNum { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public double Qty { get; set; }
    //{ 
    //    get=> _qty;
    //    set 
    //    {
    //        _qty = value;
    //        QtyLost = QtyRequire - QtyPlan - value;
    //    } 
    //}
    public double QtyRequire { get; set; }
    public double QtyPlan { get; set; }
    public double QtyManual { get; set; }
    public double QtyLost { get; set; }
    public double Price { get; set; }
    public string WhsCode { get; set; } = string.Empty;
    public string UomName { get; set; } = string.Empty;
    public string? ManageItem { get; set; }
    public int Type { get; set; }
    public List<BatchReturnComponentProduction> Batches { get; set; } = new();
    public List<SerialReturnComponentProduction> Serials { get; set; } = new();
    public List<ItemNoneReturnComponentProduction>? ItemNones { get; set; }
}

public class BatchReturnComponentProduction
{
    public string BatchCode { get; set; } = string.Empty;
    public double Qty { get; set; }
    public double QtyManual { get; set; }
    public double QtyLost { get; set; }
    public DateTime? ExpDate { get; set; } = DateTime.Today;
    public DateTime? ManfectureDate { get; set; } = DateTime.Today;
    public DateTime? AdmissionDate { get; set; } = DateTime.Today;
    public string LotNo { get; set; } = string.Empty;
    public double QtyAvailable { get; set; }
    public IEnumerable<GetBatchOrSerial> OnSelectedBatchOrSerial { get; set; } = Array.Empty<GetBatchOrSerial>();
    public IEnumerable<GetProductionOrder> OnSelectedProductionOrder { get; set; } = Array.Empty<GetProductionOrder>();
    public IEnumerable<ItemType> OnSelectedType { get; set; } = new List<ItemType> { new ItemType { Id = 1, Name = "Auto" } };
}

public class SerialReturnComponentProduction
{
    public string SerialCode { get; set; } = string.Empty;
    public int Qty { get; set; } = 1;
    public string MfrNo { get; set; } = string.Empty;
    public DateTime? MfrDate { get; set; } = DateTime.Today;
    public DateTime? ExpDate { get; set; } = DateTime.Today;
    public IEnumerable<GetBatchOrSerial> OnSelectedBatchOrSerial { get; set; } = Array.Empty<GetBatchOrSerial>();
    //public IEnumerable<GetProductionOrder> OnSelectedProductionOrder { get; set; } = Array.Empty<GetProductionOrder>();
    public IEnumerable<string> OnSelectedType { get; set; } = new List<string> { "Auto" };
}

public class ItemNoneReturnComponentProduction
{
    public IEnumerable<GetProductionOrder> OnSelectedProductionOrder { get; set; } = Array.Empty<GetProductionOrder>();
    public double Qty { get; set; }
    public double QtyLost { get; set; }
}
