namespace Piyavate_Hospital.Shared.Models.IssueForProduction;

public class IssueProductionHeader
{
    public string Series { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public DateTime? DocDate { get; set; }
    public List<IssueProductionLine> Lines { get; set; }=new();
}