using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.ReturnComponentProduction;

namespace Piyavate_Hospital.Shared.Views.ReturnComponent.MobileAppScreen.Add;

public partial class AddBatchLine
{
    [Parameter] public Func<Task> IsViewDetail { get; set; } = default!;
    [Parameter] public Func<BatchReturnComponentProduction, Task> SaveBatch { get; set; } = default!;
    [Parameter] public Func<int, Task> DeleteBatch { get; set; } = default!;
    [Parameter] public ReturnComponentProductionLine DataResult { get; set; } = new();

    [Parameter]
    public IEnumerable<GetBatchOrSerial> SerialBatchDeliveryOrders { get; set; } = new List<GetBatchOrSerial>();

    [Parameter]
    public IEnumerable<GetProductionOrder>? SelectedProductionOrder { get; set; } = new List<GetProductionOrder>();

    [Parameter] public List<BatchReturnComponentProduction> BatchReturnComponent { get; set; } = new();
    [Parameter] public int Index { get; set; }
    [Parameter] public bool IsUpdate { get; set; }
    [Parameter] public IEnumerable<GetBatchOrSerial> SelectedBatch { get; set; } = Array.Empty<GetBatchOrSerial>();
    private BatchReturnComponentProduction BatchReturnComponentProduction { get; set; } = new();

    private IEnumerable<ItemType> _type = new List<ItemType>
    {
        new ItemType
        {
            Id = 1,
            Name = "Auto"
        },
        new ItemType
        {
            Id = 2,
            Name = "Manual"
        }
    };

    private IEnumerable<ItemType> StatusSelect { get; set; } = default!;
    public bool DisplayNoneOrShow;

    private void OnSearchType(OptionsSearchEventArgs<ItemType> e)
    {
        e.Items = _type.Where(i => i.Name.Contains(e.Text, StringComparison.OrdinalIgnoreCase));
    }

    private void OnSelectedType(string newValue)
    {
        Console.WriteLine("OnSelectedType");
        Console.WriteLine(JsonSerializer.Serialize(BatchReturnComponentProduction.OnSelectedType));
        DisplayNoneOrShow = BatchReturnComponentProduction.OnSelectedType.FirstOrDefault()?.Name != "";
        StateHasChanged();
    }

    protected override void OnInitialized()
    {
        OnSelectedType("");
        Console.WriteLine(SelectedBatch.Count());
        if (SelectedBatch.Count() != 0)
        {
            UpdateItemDetails("");
            BatchReturnComponentProduction.OnSelectedType = BatchReturnComponent[Index].OnSelectedType;
            BatchReturnComponentProduction.OnSelectedProductionOrder =
                BatchReturnComponent[Index].OnSelectedProductionOrder;
            BatchReturnComponentProduction.QtyLost = BatchReturnComponent[Index].QtyLost;
            BatchReturnComponentProduction.QtyManual = BatchReturnComponent[Index].QtyManual;
        }
    }

    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("ReturnFromComponent");
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
        BatchReturnComponentProduction.BatchCode = firstItem.SerialBatch;
        BatchReturnComponentProduction.Qty =
            (string.IsNullOrEmpty(firstItem.InputQty)) ? 0 : Convert.ToDouble(firstItem.InputQty);
        BatchReturnComponentProduction.ExpDate = (!string.IsNullOrEmpty(firstItem.ExpDate))
            ? DateTime.Parse(firstItem.ExpDate)
            : BatchReturnComponentProduction.ExpDate;
        BatchReturnComponentProduction.ManfectureDate = (!string.IsNullOrEmpty(firstItem.MrfDate))
            ? DateTime.Parse(firstItem.MrfDate)
            : BatchReturnComponentProduction.ManfectureDate;
        BatchReturnComponentProduction.AdmissionDate = (!string.IsNullOrEmpty(firstItem.MrfDate))
            ? DateTime.Parse(firstItem.MrfDate)
            : BatchReturnComponentProduction.AdmissionDate;
        BatchReturnComponentProduction.QtyAvailable = Convert.ToDouble(firstItem.Qty);
        DisplayNoneOrShow = BatchReturnComponentProduction.OnSelectedType.FirstOrDefault()?.Name != "";
        return Task.CompletedTask;
    }

    private void OnSearchDocNum(OptionsSearchEventArgs<GetProductionOrder> e)
    {
        e.Items = SelectedProductionOrder?.Where(i => i.DocNum.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.DocNum);
    }

    private void OnSelectedDocument(string newValue)
    {
        //todo
        // Console.WriteLine(index);
    }

    private void OnChangeQtyManual(ChangeEventArgs e, BatchReturnComponentProduction obj)
    {
        Console.WriteLine("OnChangeQtyManual");
        Console.WriteLine(e.Value);
        if (double.TryParse(e.Value?.ToString(), out double qty))
        {
            obj.QtyLost = qty;
            DataResult.QtyManual = (BatchReturnComponent.Where(x => x.BatchCode != obj.BatchCode)
                .Sum(x => x.QtyLost) + BatchReturnComponentProduction.QtyLost);
            Console.WriteLine(DataResult.QtyManual);
            DataResult.QtyLost = (DataResult.QtyRequire - DataResult.QtyPlan - DataResult.Qty) -
                                 (BatchReturnComponent.Where(x => x.BatchCode != obj.BatchCode)
                                      .Sum(x => x.QtyLost) +
                                  BatchReturnComponentProduction.QtyLost);
            //BatchReturnComponent.Sum(x => x.QtyLost);
        }
    }
}