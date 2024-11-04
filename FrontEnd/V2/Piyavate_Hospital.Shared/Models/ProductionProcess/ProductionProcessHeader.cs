using System.Collections.ObjectModel;

namespace Piyavate_Hospital.Shared.Models.ProductionProcess;

public class ProductionProcessHeader
{
    public ObservableCollection<ProcessProductionLine> Data { get; set; }= new();
}
