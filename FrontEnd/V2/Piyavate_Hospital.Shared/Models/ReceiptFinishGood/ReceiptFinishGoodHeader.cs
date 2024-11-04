namespace Piyavate_Hospital.Shared.Models.ReceiptFinishGood;

public class ReceiptFinishGoodHeader
{
    public string Series { get; set; } = string.Empty;
    public DateTime? DocDate { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public List<ReceiptFinishGoodLine> Lines { get; set; } = new();
}