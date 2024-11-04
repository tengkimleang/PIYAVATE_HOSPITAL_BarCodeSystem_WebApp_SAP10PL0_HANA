using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piyavate_Hospital.Shared.Models.InventoryTransfer;

public class InventoryTransferHeader
{
    public string Series { get; set; } = string.Empty;
    public DateTime? DocDate { get; set; }
    public DateTime? TaxDate { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public string FromWarehouse { get; set; } = string.Empty;
    public string ToWarehouse { get; set; } = string.Empty;
    public List<InventoryTransferLine> Lines { get; set; } = new List<InventoryTransferLine>();
    public bool IsDraft { get; set; }
}
