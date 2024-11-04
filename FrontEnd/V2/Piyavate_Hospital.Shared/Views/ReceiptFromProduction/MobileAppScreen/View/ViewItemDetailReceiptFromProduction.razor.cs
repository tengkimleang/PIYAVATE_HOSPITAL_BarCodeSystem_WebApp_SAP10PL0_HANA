using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;

namespace Piyavate_Hospital.Shared.Views.ReceiptFromProduction.MobileAppScreen.View;

public partial class ViewItemDetailReceiptFromProduction
{
    [Parameter] public GoodReceiptPoLineByDocNum ItemDetail { get; set; } = new();
    [Parameter] public List<GetBatchOrSerial> GetBatchOrSerials { get; set; } = new();

    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("ReceiptsFinishedGoods");
        }
    }

    [Parameter] public Func<Task> IsViewDetailBack { get; set; } = default!;
}