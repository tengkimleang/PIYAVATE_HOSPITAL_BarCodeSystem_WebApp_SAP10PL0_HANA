
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Collections.ObjectModel;
using Piyavate_Hospital.Shared.Models.Gets;

namespace Piyavate_Hospital.Shared.Views.InventoryTransfer;

public partial class ViewInventoryTransferForm
{
    [Parameter]
    public Func<Task> AddNew { get; set; } = default!;
    [Parameter]
    public Func<Task> Search { get; set; } = default!;
    [Parameter]
    public Func<Task> OnViewBatchOrSerial { get; set; } = default!;
    [Parameter]
    public GoodReceiptPoHeaderDeatialByDocNum? GetGoodReceiptPoHeaderDetailByDocNum { get; set; }
    [Parameter]
    public ObservableCollection<GoodReceiptPoLineByDocNum> GoodReceiptPoLineByDocNums { get; set; } = new ObservableCollection<GoodReceiptPoLineByDocNum>();
    string dataGrid = "width: 100%;height:405px";
    void UpdateGridSize(GridItemSize size)
    {
        if (size == GridItemSize.Xs)
        {
            dataGrid = "width: 700px;height:205px";
        }
        else
        {
            dataGrid = "width: 100%;height:405px";
        }
    }
}