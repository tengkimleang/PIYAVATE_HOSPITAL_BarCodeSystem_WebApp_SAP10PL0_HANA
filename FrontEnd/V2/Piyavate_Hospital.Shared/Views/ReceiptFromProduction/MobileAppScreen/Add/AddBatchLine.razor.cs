using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.GoodReceiptPo;
using Piyavate_Hospital.Shared.Models.ReceiptFinishGood;

namespace Piyavate_Hospital.Shared.Views.ReceiptFromProduction.MobileAppScreen.Add;

public partial class AddBatchLine
{
    [Parameter] public Func<Task> IsViewDetail { get; set; } = default!;
    [Parameter] public Func<ReceiptFinishGoodBatch, Task> SaveBatch { get; set; } = default!;
    [Parameter] public Func<int, Task> DeleteBatch { get; set; } = default!;
    [Parameter] public Func<Task<string>> GetGenerateBatchSerial { get; set; } = default!;

    [Parameter]
    public IEnumerable<GetBatchOrSerial> SerialBatchDeliveryOrders { get; set; } = new List<GetBatchOrSerial>();

    [Parameter] public int Index { get; set; }

    [Parameter] public bool IsUpdate { get; set; }

    [Parameter] public IEnumerable<GetBatchOrSerial> SelectedBatch { get; set; } = Array.Empty<GetBatchOrSerial>();
    private ReceiptFinishGoodBatch BatchReceiptPo { get; set; } = new();

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
            NavigationManager.NavigateTo("ReceiptsFinishedGoods");
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
        Console.WriteLine(JsonSerializer.Serialize(firstItem));
        if (firstItem == null) return Task.CompletedTask;
        Console.WriteLine(JsonSerializer.Serialize(firstItem));
        BatchReceiptPo.BatchCode = firstItem.SerialBatch;
        BatchReceiptPo.Qty = (string.IsNullOrEmpty(firstItem.InputQty)) ? 0 : Convert.ToDouble(firstItem.InputQty);
        BatchReceiptPo.ExpDate = (!string.IsNullOrEmpty(firstItem.ExpDate))
            ? DateTime.Parse(firstItem.ExpDate)
            : BatchReceiptPo.ExpDate;
        BatchReceiptPo.ManufactureDate = (!string.IsNullOrEmpty(firstItem.MrfDate))
            ? DateTime.Parse(firstItem.MrfDate)
            : BatchReceiptPo.ManufactureDate;
        BatchReceiptPo.AdmissionDate = (!string.IsNullOrEmpty(firstItem.MrfDate))
            ? DateTime.Parse(firstItem.MrfDate)
            : BatchReceiptPo.AdmissionDate;
        // BatchReceiptPo.QtyAvailable = Convert.ToDouble(firstItem.Qty);
        return Task.CompletedTask;
    }

    private async Task OnClickGenerateBatchSerial()
    {
        BatchReceiptPo.BatchCode = (await GetGenerateBatchSerial());
    }
}