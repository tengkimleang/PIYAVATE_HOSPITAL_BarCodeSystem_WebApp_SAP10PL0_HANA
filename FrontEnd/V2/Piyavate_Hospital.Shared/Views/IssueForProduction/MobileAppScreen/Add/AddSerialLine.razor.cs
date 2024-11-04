

using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.IssueForProduction;

namespace Piyavate_Hospital.Shared.Views.IssueForProduction.MobileAppScreen.Add;

public partial class AddSerialLine
{
    [Parameter] public Func<Task> IsViewDetail { get; set; } = default!;
    [Parameter] public Func<SerialIssueProduction, Task> SaveSerial { get; set; } = default!;
    [Parameter] public Func<int,Task> DeleteSerial { get; set; } = default!;

    [Parameter]
    public IEnumerable<GetBatchOrSerial> SerialBatchDeliveryOrders { get; set; } = new List<GetBatchOrSerial>();

    [Parameter] public int Index { get; set; }

    [Parameter] public bool IsUpdate { get; set; }

    [Parameter] public IEnumerable<GetBatchOrSerial> SelectedSerial { get; set; } = Array.Empty<GetBatchOrSerial>();
    private SerialIssueProduction SerialIssueProduction { get; set; } = new();

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
        SerialIssueProduction.SerialCode = firstItem.SerialBatch;
        SerialIssueProduction.Qty = 1;
        SerialIssueProduction.ExpDate = (!string.IsNullOrEmpty(firstItem.ExpDate))
            ? DateTime.Parse(firstItem.ExpDate)
            : SerialIssueProduction.ExpDate;
        SerialIssueProduction.MfrDate = (!string.IsNullOrEmpty(firstItem.MrfDate))
            ? DateTime.Parse(firstItem.MrfDate)
            : SerialIssueProduction.MfrDate;
        return Task.CompletedTask;
    }
}