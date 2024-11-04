using System.Collections.ObjectModel;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.InventoryCounting;
using Piyavate_Hospital.Shared.Models.IssueForProduction;

namespace Piyavate_Hospital.Shared.Views.InventoryCounting;

public partial class DialogAddLineInventoryCounting
{
    [Inject] public IValidator<InventoryCountingLine>? Validator { get; init; }

    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;

    [Parameter] public Dictionary<string, object> Content { get; set; } = default!;

    private InventoryCountingLine DataResult { get; set; } = new();
    private IEnumerable<Warehouses>? Warehouses => Content["warehouse"] as IEnumerable<Warehouses>;
    private List<InventoryCountingBatch> _batchReceiptPo = new();
    private List<InventoryCountingSerial> _serialReceiptPo = new();
    private bool IsItemBatch;
    private bool IsItemSerial;
    private IEnumerable<GetBatchOrSerial> _serialBatchDeliveryOrders = new List<GetBatchOrSerial>();
    private IEnumerable<GetBatchOrSerial> _selectedSerialDeliveryOrders = Array.Empty<GetBatchOrSerial>();

    private Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> GetSerialBatch =>
        Content["getSerialBatch"] as Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> ??
        default!;

    private IEnumerable<GetInventoryCountingLines> _selectedItem = Array.Empty<GetInventoryCountingLines>();

    private IEnumerable<GetInventoryCountingLines> ListGetProductionOrderLines =>
        Content["item"] as IEnumerable<GetInventoryCountingLines> ?? new List<GetInventoryCountingLines>();

    string? dataGrid = "width: 1600px;";

    protected override async void OnInitialized()
    {
        if (Content.TryGetValue("line", out var value))
        {
            DataResult = value as InventoryCountingLine ?? new InventoryCountingLine();
            _batchReceiptPo = DataResult.Batches ?? new List<InventoryCountingBatch>();
            _serialReceiptPo = DataResult.Serials ?? new List<InventoryCountingSerial>();
            _selectedItem = ListGetProductionOrderLines.Where(i => i.ItemCode == DataResult.ItemCode);
            await UpdateItemDetails(DataResult.ItemCode);
        }
    }

    private void OnSearch(OptionsSearchEventArgs<GetInventoryCountingLines> e)
    {
        e.Items = ListGetProductionOrderLines?.Where(i =>
                i.ItemCode.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                i.ItemName.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.ItemCode);
    }

    private async Task SaveAsync()
    {
        DataResult.Batches = _batchReceiptPo;
        DataResult.Serials = _serialReceiptPo;
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

    private async Task UpdateItemDetails(string? newValue)
    {
        var firstItem = _selectedItem.FirstOrDefault();
        Console.WriteLine(JsonSerializer.Serialize(_selectedItem));
        //ListGetProductionOrderLines
        Console.WriteLine(JsonSerializer.Serialize(ListGetProductionOrderLines));
        DataResult.ItemCode = firstItem?.ItemCode ?? "";
        DataResult.ItemName = firstItem?.ItemName ?? "";
        DataResult.WhsCode = firstItem?.WarehouseCode ?? "";
        DataResult.Uom = firstItem?.Uom ?? "";
        DataResult.ManageItem = firstItem?.ItemType??"N";
        DataResult.Qty = Convert.ToDouble(firstItem?.Qty ?? "0");
        IsItemBatch = firstItem?.ItemType == "B";
        IsItemSerial = firstItem?.ItemType == "S";
        if (firstItem?.ItemType != "N")
            _serialBatchDeliveryOrders = await GetSerialBatch(new Dictionary<string, string>
            {
                { "ItemCode", firstItem?.ItemCode ?? "" },
                { "ItemType", firstItem?.ItemType ?? "" }
            });
    }

    private void UpdateGridSize(GridItemSize size)
    {
        dataGrid = size == GridItemSize.Xs ? "width: 1200px;height:205px" : "width: 1600px;height:405px";
    }

    private void AddLineToBatchOrSerial()
    {
        if (IsItemBatch)
        {
            _batchReceiptPo.Add(new InventoryCountingBatch { BatchCode = "", });
        }
        else if (IsItemSerial && _serialReceiptPo.Count() < DataResult.QtyCounted)
        {
            _serialReceiptPo.Add(new InventoryCountingSerial { SerialCode = "", Qty = 1, });
        }
    }

    private void DeleteLineFromBatchOrSerial(int index)
    {
        if (IsItemBatch)
        {
            _batchReceiptPo.RemoveAt(index);
        }
        else if (IsItemSerial)
        {
            _serialReceiptPo.RemoveAt(index);
        }
    }

    private void UpdateFromBatchOrSerial(int index)
    {
        if (IsItemBatch)
        {
            var condition = _batchReceiptPo[index].IsBatchNew == "Y" ? "N" : "Y";
            _batchReceiptPo[index]=new ()
            {
                IsBatchNew = condition,
                Qty = 1,
            };
        }
        else if (IsItemSerial)
        {
            var condition = _serialReceiptPo[index].IsSerialNew == "Y" ? "N" : "Y";
            _serialReceiptPo[index]=new ()
            {
                IsSerialNew = condition,
                Qty = 1,
            };
        }
    }

    private void OnSearchSerial(OptionsSearchEventArgs<GetBatchOrSerial> e)
    {
        e.Items = _serialBatchDeliveryOrders?.Where(i =>
                i.SerialBatch.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                i.SerialBatch.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.SerialBatch);
    }

    private void OnSelectedSerialOrBatch(string newValue, int index, string type)
    {
        if (type == "Batch")
        {
            var firstItem = _batchReceiptPo[index].OnSelectedBatchOrSerial.FirstOrDefault();
            Console.WriteLine(
                JsonSerializer.Serialize(_batchReceiptPo[index].OnSelectedBatchOrSerial.FirstOrDefault()));
            if (firstItem != null)
            {
                _batchReceiptPo[index].BatchCode = firstItem.SerialBatch ?? string.Empty;
                _batchReceiptPo[index].Qty = 0;
                _batchReceiptPo[index].QtyAvailable = Convert.ToInt32(firstItem.Qty);
                _batchReceiptPo[index].ManufactureDate = DateTime.Parse(firstItem.MrfDate ?? string.Empty);
                _batchReceiptPo[index].ExpireDate = DateTime.Parse(firstItem.ExpDate ?? string.Empty);
            }
        }
        else if (type == "Serial")
        {
            var firstItem = _serialReceiptPo[index].OnSelectedBatchOrSerial.FirstOrDefault();
            if (firstItem != null)
            {
                _serialReceiptPo[index].SerialCode = firstItem.SerialBatch ?? string.Empty;
                _serialReceiptPo[index].Qty = 1;
                _serialReceiptPo[index].MfrDate = DateTime.Parse(firstItem.MrfDate ?? string.Empty);
                _serialReceiptPo[index].ExpDate = DateTime.Parse(firstItem.ExpDate ?? string.Empty);
                _serialReceiptPo[index].MfrNo = firstItem.MfrSerialNo;
            }
        }
    }
}