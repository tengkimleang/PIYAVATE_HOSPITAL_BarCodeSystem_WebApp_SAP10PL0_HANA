

using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Collections.ObjectModel;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.InventoryTransfer;

namespace Piyavate_Hospital.Shared.Views.InventoryTransfer;

public partial class DialogAddLineInventoryTrasfer
{
    [Inject]
    public IValidator<InventoryTransferLine>? Validator { get; init; }

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    [Parameter]
    public Dictionary<string, object> Content { get; set; } = default!;

    private InventoryTransferLine DataResult { get; set; } = new();
    private List<BatchInventoryTransfer> _batchReceiptPo = new();
    private List<SerialInventoryTransfer> _serialReceiptPo = new();
    private IEnumerable<GetBatchOrSerial> _serialBatchDeliveryOrders = new List<GetBatchOrSerial>();
    private bool _isItemBatch;
    private bool _isItemSerial;
    private IEnumerable<Items> _selectedItem = Array.Empty<Items>();
    private IEnumerable<GetBatchOrSerial> _selectedSerialDeliveryOrders = Array.Empty<GetBatchOrSerial>();
    private IEnumerable<Items> _item => Content["item"] as IEnumerable<Items> ?? new List<Items>();

    private Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> GetSerialBatch => Content["getSerialBatch"] as Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> ?? default!;

    string? dataGrid = "width: 1600px;overflow-x:scroll;";

    protected override async void OnInitialized()
    {
        if (Content.TryGetValue("line", out var value))
        {
            DataResult = value as InventoryTransferLine ?? new InventoryTransferLine();
            _batchReceiptPo = DataResult.Batches ?? new List<BatchInventoryTransfer>();
            _serialReceiptPo = DataResult.Serials ?? new List<SerialInventoryTransfer>();
            _selectedItem = _item.Where(i => i.ItemCode == DataResult.ItemCode);
            await UpdateItemDetails(DataResult.ItemCode);
        }
    }

    private void OnSearch(OptionsSearchEventArgs<Items> e)
    {
        e.Items = _item.Where(i => i.ItemCode.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                            i.ItemName.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
                            .OrderBy(i => i.ItemCode);
    }

    private void OnSearchSerial(OptionsSearchEventArgs<GetBatchOrSerial> e)
    {
        e.Items = _serialBatchDeliveryOrders?.Where(i => i.SerialBatch.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                                                    i.SerialBatch.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.SerialBatch);
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

    private async Task UpdateItemDetails(string newValue)
    {
        var firstItem = _selectedItem.FirstOrDefault();
        DataResult.ItemCode = firstItem?.ItemCode ?? "";
        DataResult.ItemName = firstItem?.ItemName ?? "";
        DataResult.ManageItem = firstItem?.ItemType??"N";
        _isItemBatch = firstItem?.ItemType == "B";
        _isItemSerial = firstItem?.ItemType == "S";
        if (firstItem?.ItemType != "N")
            _serialBatchDeliveryOrders = await GetSerialBatch(new Dictionary<string, string>
                {
                    {"ItemCode", firstItem?.ItemCode ??""},
                    {"ItemType", firstItem?.ItemType ??""}
                });
    }
    private void OnSelectedSerialOrBatch(string newValue, int index, string type)
    {

        if (type == "Batch")
        {
            var firstItem = _batchReceiptPo[index].OnSelectedBatchOrSerial.FirstOrDefault();
            if (firstItem != null)
            {
                _batchReceiptPo[index].BatchCode = firstItem.SerialBatch ?? string.Empty;
                _batchReceiptPo[index].Qty = 0;
                _batchReceiptPo[index].QtyAvailable = Convert.ToInt32(firstItem.Qty);
                _batchReceiptPo[index].ManfectureDate = DateTime.Parse(firstItem.MrfDate ?? string.Empty);
                _batchReceiptPo[index].ExpDate = DateTime.Parse(firstItem.ExpDate ?? string.Empty);
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

    private void AddLineToBatchOrSerial()
    {
        if (_isItemBatch)
        {
            _batchReceiptPo.Add(new BatchInventoryTransfer { BatchCode = "", });
        }
        else if (_isItemSerial && _serialReceiptPo.Count() < DataResult.Qty)
        {
            _serialReceiptPo.Add(new SerialInventoryTransfer { SerialCode = "", });
        }
    }

    private void DeleteLineFromBatchOrSerial(int index)
    {
        if (_isItemBatch)
        {
            _batchReceiptPo.RemoveAt(index);
        }
        else if (_isItemSerial)
        {
            _serialReceiptPo.RemoveAt(index);
        }
    }

    private void UpdateGridSize(GridItemSize size)
    {
        dataGrid = size == GridItemSize.Xs ? "width: 1600px;height:205px" : "width: 1600px;overflow-x:scroll;max-height: 400px;";
    }
}
