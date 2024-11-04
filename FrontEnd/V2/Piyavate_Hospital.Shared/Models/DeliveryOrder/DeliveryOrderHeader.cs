

namespace Piyavate_Hospital.Shared.Models.DeliveryOrder;

public class DeliveryOrderHeader
{
    public string CustomerCode { get; set; } = string.Empty;
    public string ContactPersonCode { get; set; } = "0";
    public string NumAtCard { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public DateTime? DocDate { get; set; }
    public DateTime? TaxDate { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public bool IsDraft { get; set; } = false;
    public List<DeliveryOrderLine>? Lines { get; set; }
}
