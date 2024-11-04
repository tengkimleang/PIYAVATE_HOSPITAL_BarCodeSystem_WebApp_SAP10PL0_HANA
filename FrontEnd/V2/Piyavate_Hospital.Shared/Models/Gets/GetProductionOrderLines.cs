namespace Piyavate_Hospital.Shared.Models.Gets;

public class GetProductionOrderLines
{
    public string DocEntry { get; set; } = string.Empty;
    public string OrderLineNum { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Qty { get; set; } = string.Empty;
    public string PlanQty { get; set; } = string.Empty;
    public string Uom { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
}