
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;

namespace Piyavate_Hospital.Shared.Views.GoodReturn.MobileAppScreen.Add;

public partial class AddBatchLine
{
    [Parameter] public Func<Task> IsViewDetail { get; set; } = default!;
    [Parameter] public Func<BatchDeliveryOrder, Task> SaveBatch { get; set; } = default!;
    [Parameter] public Func<int,Task> DeleteBatch { get; set; } = default!;

    [Parameter]
    public IEnumerable<GetBatchOrSerial> SerialBatchDeliveryOrders { get; set; } = new List<GetBatchOrSerial>();

    [Parameter] public int Index { get; set; }

    [Parameter] public bool IsUpdate { get; set; }

    [Parameter] public IEnumerable<GetBatchOrSerial> SelectedBatch { get; set; } = Array.Empty<GetBatchOrSerial>();
    private BatchDeliveryOrder BatchDeliveryOrder { get; set; } = new();

    protected override void OnInitialized()
    {
        Console.WriteLine(SelectedBatch.Count());
        if (SelectedBatch.Count() != 0)
            UpdateItemDetails("");
    }

    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("return");
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
        var firstItem = SelectedBatch.FirstOrDefault();
        if (firstItem == null) return Task.CompletedTask;
        Console.WriteLine(JsonSerializer.Serialize(firstItem));
        BatchDeliveryOrder.BatchCode = firstItem.SerialBatch;
        BatchDeliveryOrder.Qty = (string.IsNullOrEmpty(firstItem.InputQty)) ? 0 : Convert.ToDouble(firstItem.InputQty);
        BatchDeliveryOrder.ExpDate = (!string.IsNullOrEmpty(firstItem.ExpDate))
            ? DateTime.Parse(firstItem.ExpDate)
            : BatchDeliveryOrder.ExpDate;
        BatchDeliveryOrder.ManfectureDate = (!string.IsNullOrEmpty(firstItem.MrfDate))
            ? DateTime.Parse(firstItem.MrfDate)
            : BatchDeliveryOrder.ManfectureDate;
        BatchDeliveryOrder.AdmissionDate = (!string.IsNullOrEmpty(firstItem.MrfDate))
            ? DateTime.Parse(firstItem.MrfDate)
            : BatchDeliveryOrder.AdmissionDate;
        BatchDeliveryOrder.QtyAvailable = Convert.ToDouble(firstItem.Qty);
        return Task.CompletedTask;
    }
}