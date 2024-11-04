using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.ReceiptFinishGood;

namespace Piyavate_Hospital.Shared.Views.ReceiptFromProduction;

public partial class DialogAddLineReceiptFromProductionOrder
{
    [Inject] public IValidator<ReceiptFinishGoodLine>? Validator { get; init; }

    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;

    [Parameter] public Dictionary<string, object> Content { get; set; } = default!;

    private ReceiptFinishGoodLine DataResult { get; set; } = new();
    private List<ReceiptFinishGoodBatch> _batchReceiptPOs = new();
    private List<ReceiptFinishGoodSerial> _serialReceiptPo = new();
    private bool _isItemBatch;
    private bool _isItemSerial;
    private IEnumerable<GetProductionOrderLines> _selectedItem = Array.Empty<GetProductionOrderLines>();

    private IEnumerable<GetProductionOrderLines> Items => Content["item"] as IEnumerable<GetProductionOrderLines> ??
                                                          new List<GetProductionOrderLines>();

    private Func<Dictionary<string, object>, Task<string>> GetGenerateBatchSerial =>
        Content["getGenerateBatchSerial"] as Func<Dictionary<string, object>, Task<string>> ?? default!;

    string? _dataGrid = "width: 1600px;";

    protected override void OnInitialized()
    {
        if (Content.TryGetValue("line", out var value))
        {
            DataResult = value as ReceiptFinishGoodLine ?? new ReceiptFinishGoodLine();
            _batchReceiptPOs = DataResult.Batches;
            _serialReceiptPo = DataResult.Serials;
            _selectedItem = Items.Where(i => i.ItemCode == DataResult.ItemCode);
            UpdateItemDetails("update");
        }
    }

    private void OnSearch(OptionsSearchEventArgs<GetProductionOrderLines> e)
    {
        e.Items = Items.Where(i => i.ItemCode.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                                   i.ItemName.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.ItemCode);
    }

    private async Task SaveAsync()
    {
        DataResult.Batches = _batchReceiptPOs;
        DataResult.Serials = _serialReceiptPo;
        DataResult.DocNum = Convert.ToInt32(_selectedItem.FirstOrDefault()?.DocEntry ?? "");
        var result = await Validator!.ValidateAsync(DataResult).ConfigureAwait(false);
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                ToastService!.ShowError(error.ErrorMessage);
            }

            return;
        }

        await Dialog.CloseAsync(new Dictionary<string, object>
        {
            { "data", DataResult },
            { "isUpdate", Content.ContainsKey("line") }
        });
    }

    private void UpdateItemDetails(string valueNew = "")
    {
        var firstItem = _selectedItem.FirstOrDefault();
        Console.WriteLine(JsonSerializer.Serialize(_selectedItem));
        DataResult.ItemCode = firstItem?.ItemCode ?? "";
        DataResult.ItemName = firstItem?.ItemName ?? "";
        DataResult.Qty = string.IsNullOrEmpty(valueNew) ? Convert.ToDouble(firstItem?.Qty ?? "0"):  DataResult.Qty;
        DataResult.QtyPlan = Convert.ToDouble(firstItem?.PlanQty ?? "0");
        DataResult.WhsCode = firstItem?.WarehouseCode ?? "";
        DataResult.UomName = firstItem?.Uom ?? "";
        DataResult.ManageItem = firstItem?.ItemType ?? "";
        _isItemBatch = firstItem?.ItemType == "B";
        _isItemSerial = firstItem?.ItemType == "S";
    }

    private void AddLineToBatchOrSerial()
    {
        if (_isItemBatch)
        {
            _batchReceiptPOs.Add(new ReceiptFinishGoodBatch { BatchCode = "", });
        }
        else if (_isItemSerial && _serialReceiptPo.Count() < DataResult.Qty)
        {
            _serialReceiptPo.Add(new ReceiptFinishGoodSerial { SerialCode = "", });
        }
    }

    private void DeleteLineFromBatchOrSerial(int index)
    {
        if (_isItemBatch)
        {
            _batchReceiptPOs.RemoveAt(index);
        }
        else if (_isItemSerial)
        {
            _serialReceiptPo.RemoveAt(index);
        }
    }

    private void UpdateGridSize(GridItemSize size)
    {
        _dataGrid = size == GridItemSize.Xs ? "width: 1200px;height:205px" : "width: 100%;height:405px";
    }

    private async Task OnClickGennerateBatchSerial(int index)
    {
        var data = new Dictionary<string, object>
        {
            { "itemCode", DataResult.ItemCode },
            { "qty", DataResult.Qty }
        };
        if (_isItemBatch)
            _batchReceiptPOs[index].BatchCode = (await GetGenerateBatchSerial(data));
        else if (_isItemSerial)
            _serialReceiptPo[index].SerialCode = await GetGenerateBatchSerial(data);
    }
}