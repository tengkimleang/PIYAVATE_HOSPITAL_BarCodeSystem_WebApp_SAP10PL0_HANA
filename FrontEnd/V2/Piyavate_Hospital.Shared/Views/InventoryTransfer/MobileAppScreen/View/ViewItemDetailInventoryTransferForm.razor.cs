
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;

namespace Piyavate_Hospital.Shared.Views.InventoryTransfer.MobileAppScreen.View;

public partial class ViewItemDetailInventoryTransferForm
{
    [Parameter] public GoodReceiptPoLineByDocNum ItemDetail { get; set; } = new();
    [Parameter] public List<GetBatchOrSerial> GetBatchOrSerials { get; set; } = new();

    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("inventorytransfer");
        }
    }

    [Parameter] public Func<Task> IsViewDetailBack { get; set; } = default!;
}