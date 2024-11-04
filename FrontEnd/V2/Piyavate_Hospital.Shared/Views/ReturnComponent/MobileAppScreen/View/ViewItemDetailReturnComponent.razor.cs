
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;

namespace Piyavate_Hospital.Shared.Views.ReturnComponent.MobileAppScreen.View;

public partial class ViewItemDetailReturnComponent
{
    [Parameter] public GoodReceiptPoLineByDocNum ItemDetail { get; set; } = default!;
    [Parameter] public List<GetBatchOrSerial> GetBatchOrSerials { get; set; } = new();

    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("ReturnFromComponent");
        }
    }

    [Parameter] public Func<Task> IsViewDetailBack { get; set; } = default!;
}