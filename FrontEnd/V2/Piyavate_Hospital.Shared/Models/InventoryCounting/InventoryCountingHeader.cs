namespace Piyavate_Hospital.Shared.Models.InventoryCounting;

public class InventoryCountingHeader
{
    public int DocEntry { get; set; }
    public string CreateTime { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; }
    public string OtherRemark { get; set; } = string.Empty;
    public string Ref2 { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public string InventoryCountingType { get; set; } = string.Empty;
    public int CounterId { get; set; }
    public List<InventoryCountingLine> Lines { get; set; } = new();
}