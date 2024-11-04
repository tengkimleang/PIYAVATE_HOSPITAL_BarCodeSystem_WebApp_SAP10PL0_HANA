namespace Piyavate_Hospital.Shared.Models.ReturnComponentProduction;

public class ReturnComponentProductionHeader
{
    public string Series { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public DateTime? DocDate { get; set; }
    public List<ReturnComponentProductionLine> Lines { get; set; }=new();
}