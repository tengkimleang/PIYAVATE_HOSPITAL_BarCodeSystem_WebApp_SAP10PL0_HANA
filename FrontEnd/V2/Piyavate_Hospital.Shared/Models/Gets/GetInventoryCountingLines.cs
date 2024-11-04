namespace Piyavate_Hospital.Shared.Models.Gets;

public class GetInventoryCountingLines
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Qty { get; set; } = string.Empty;
    public string Counted { get; set; } = string.Empty;
    public string LineNum { get; set; }  = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string CountId { get; set; } = string.Empty;
    public string BinEntry { get; set; } = string.Empty;
    public string Uom { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
}