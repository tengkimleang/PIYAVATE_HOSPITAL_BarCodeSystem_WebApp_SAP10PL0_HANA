

using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;

namespace Piyavate_Hospital.Shared.Views.InventoryCounting.MobileAppScreen.View;

public partial class ViewItemDetailInventoryCounting
{
    [Parameter] public GetDetailInventoryCountingLineByDocNum ItemDetail { get; set; } = default!;
    [Parameter] public List<GetBatchOrSerial> GetBatchOrSerials { get; set; } = new();

    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("inventorycounting");
        }
    }

    [Parameter] public Func<Task> IsViewDetailBack { get; set; } = default!;
}