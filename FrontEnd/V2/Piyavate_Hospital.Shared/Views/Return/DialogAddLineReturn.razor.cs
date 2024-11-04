using System.Collections.ObjectModel;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Extensions;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;

namespace Piyavate_Hospital.Shared.Views.Return;

public partial class DialogAddLineReturn
{
    [Inject] public IValidator<DeliveryOrderLine>? Validator { get; init; }

    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;

    [Parameter] public Dictionary<string, object> Content { get; set; } = default!;

    private DeliveryOrderLine DataResult { get; set; } = new();
    private List<BatchDeliveryOrder> _batchReceiptPo = new();
    private List<SerialDeliveryOrder> _serialReceiptPo = new();
    private IEnumerable<GetBatchOrSerial> _serialBatchDeliveryOrders = new List<GetBatchOrSerial>();
    private bool _isItemBatch;
    private bool _isItemSerial;
    private IEnumerable<Items> _selectedItem = Array.Empty<Items>();
    private IEnumerable<GetBatchOrSerial> _selectedSerialDeliveryOrders = Array.Empty<GetBatchOrSerial>();
    private IEnumerable<Items> _item => Content["item"] as IEnumerable<Items> ?? new List<Items>();

    private Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> GetSerialBatch =>
        Content["getSerialBatch"] as Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> ??
        default!;

    private IEnumerable<VatGroups>? _vatGroup => Content["taxPurchase"] as IEnumerable<VatGroups>;
    private IEnumerable<Warehouses>? _warehouses => Content["warehouse"] as IEnumerable<Warehouses>;

    string? dataGrid = "width: 1600px;";

    protected override async void OnInitialized()
    {
        if (Content.TryGetValue("line", out var value))
        {
            DataResult = value as DeliveryOrderLine ?? new DeliveryOrderLine();
            _batchReceiptPo = DataResult.Batches ?? new List<BatchDeliveryOrder>();
            _serialReceiptPo = DataResult.Serials ?? new List<SerialDeliveryOrder>();
            _selectedItem = _item.Where(i => i.ItemCode == DataResult.ItemCode);
            Console.WriteLine(JsonSerializer.Serialize(_batchReceiptPo));
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
        e.Items = _serialBatchDeliveryOrders?.Where(i =>
                i.SerialBatch.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
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
        DataResult.Price = (DataResult.Price == 0) ? double.Parse(firstItem?.PriceUnit ?? "0") : DataResult.Price;
        DataResult.ItemCode = firstItem?.ItemCode ?? "";
        DataResult.ItemName = firstItem?.ItemName ?? "";
        DataResult.ManageItem = firstItem?.ItemType;
        _isItemBatch = firstItem?.ItemType == "B";
        _isItemSerial = firstItem?.ItemType == "S";
        if (firstItem?.ItemType != "N")
        {
            _serialBatchDeliveryOrders = await GetSerialBatch(new Dictionary<string, string>
            {
                { "ItemCode", firstItem?.ItemCode ?? "" },
                { "ItemType", firstItem?.ItemType ?? "" }
            });
            if (_isItemBatch == true)
            {
                var tmpBatch = _serialBatchDeliveryOrders.ToList();
                foreach (var objBatch in _batchReceiptPo)
                {
                    tmpBatch.Add(new GetBatchOrSerial(
                        DataResult.ItemCode,
                        objBatch.Qty.ToString(),
                        objBatch.BatchCode,
                        objBatch.LotNo,
                        objBatch.ExpDate.ToIsoDateString(),
                        objBatch.ManfectureDate.ToIsoDateString(),
                        "B",
                        DataResult.LineNum.ToString()
                    ));
                }

                _serialBatchDeliveryOrders = tmpBatch.AsEnumerable();
            }
            else if (_isItemSerial == true)
            {
                var tmpBatch = _serialBatchDeliveryOrders.ToList();
                foreach (var objSerial in _serialReceiptPo)
                {
                    tmpBatch.Add(new GetBatchOrSerial(
                        DataResult.ItemCode,
                        objSerial.Qty.ToString(),
                        objSerial.SerialCode,
                        objSerial.MfrNo,
                        objSerial.ExpDate.ToIsoDateString(),
                        objSerial.MfrDate.ToIsoDateString(),
                        "S",
                        DataResult.LineNum.ToString()
                    ));
                }

                _serialBatchDeliveryOrders = tmpBatch.AsEnumerable();
            }

            Console.WriteLine("Hello Get And Update");
            Console.WriteLine(JsonSerializer.Serialize(_serialBatchDeliveryOrders));
        }
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
            _batchReceiptPo.Add(new BatchDeliveryOrder { BatchCode = "", });
        }
        else if (_isItemSerial && _serialReceiptPo.Count() < DataResult.Qty)
        {
            _serialReceiptPo.Add(new SerialDeliveryOrder { SerialCode = "", });
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
        dataGrid = size == GridItemSize.Xs ? "width: 1200px;height:205px" : "width: 100%;height:405px";
    }
}