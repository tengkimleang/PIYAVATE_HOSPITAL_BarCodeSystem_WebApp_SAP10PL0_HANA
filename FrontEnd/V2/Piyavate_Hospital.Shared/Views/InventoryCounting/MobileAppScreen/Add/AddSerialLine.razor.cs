

using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.InventoryCounting;

namespace Piyavate_Hospital.Shared.Views.InventoryCounting.MobileAppScreen.Add;

public partial class AddSerialLine
{
    [Parameter] public Func<Task> IsViewDetail { get; set; } = default!;
    [Parameter] public Func<InventoryCountingSerial, Task> SaveSerial { get; set; } = default!;
    [Parameter] public Func<int,Task> DeleteSerial { get; set; } = default!;

    [Parameter]
    public IEnumerable<GetBatchOrSerial> SerialBatchDeliveryOrders { get; set; } = new List<GetBatchOrSerial>();

    [Parameter] public int Index { get; set; }

    [Parameter] public bool IsUpdate { get; set; }

    [Parameter] public IEnumerable<GetBatchOrSerial> SelectedSerial { get; set; } = Array.Empty<GetBatchOrSerial>();
    private InventoryCountingSerial InventoryCountingSerial { get; set; } = new();

    protected override void OnInitialized()
    {
        Console.WriteLine(SelectedSerial.Count());
        if (SelectedSerial.Count() != 0)
            UpdateItemDetails("");
    }

    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("inventorycounting");
        }
    }

    private void OnSearchBatch(OptionsSearchEventArgs<GetBatchOrSerial> e)
    {
        e.Items = SerialBatchDeliveryOrders
            .Where(i => i.SerialBatch.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.SerialBatch);
    }

    private Task UpdateItemDetails(string newValue)
    {
        var firstItem = SelectedSerial.FirstOrDefault();
        if (firstItem == null) return Task.CompletedTask;
        Console.WriteLine(JsonSerializer.Serialize(firstItem));
        InventoryCountingSerial.SerialCode = firstItem.SerialBatch;
        InventoryCountingSerial.Qty = 1;
        InventoryCountingSerial.ExpDate = (!string.IsNullOrEmpty(firstItem.ExpDate))
            ? DateTime.Parse(firstItem.ExpDate)
            : InventoryCountingSerial.ExpDate;
        InventoryCountingSerial.MfrDate = (!string.IsNullOrEmpty(firstItem.MrfDate))
            ? DateTime.Parse(firstItem.MrfDate)
            : InventoryCountingSerial.MfrDate;
        return Task.CompletedTask;
    }
}