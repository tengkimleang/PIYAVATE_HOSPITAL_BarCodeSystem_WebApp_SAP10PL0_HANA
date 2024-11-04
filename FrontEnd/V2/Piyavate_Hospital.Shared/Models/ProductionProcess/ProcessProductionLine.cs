namespace Piyavate_Hospital.Shared.Models.ProductionProcess;

public class ProcessProductionLine
{
    public int Index { get; set; }
    public string DocNum { get; set; } = string.Empty;
    public int ProductionNo { get; set; }
    public string ProcessStage { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}