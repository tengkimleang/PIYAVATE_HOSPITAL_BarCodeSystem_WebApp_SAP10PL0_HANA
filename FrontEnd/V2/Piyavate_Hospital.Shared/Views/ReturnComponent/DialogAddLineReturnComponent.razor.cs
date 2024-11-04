
using System.Collections.ObjectModel;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.ReturnComponentProduction;
using Piyavate_Hospital.Shared.Views.ReceiptFromProduction;

namespace Piyavate_Hospital.Shared.Views.ReturnComponent;

public partial class DialogAddLineReturnComponent
{
    [Inject] public IValidator<ReturnComponentProductionLine>? Validator { get; init; }

    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;

    [Parameter] public Dictionary<string, object> Content { get; set; } = default!;

    private ReturnComponentProductionLine DataResult { get; set; } = new();

    private IEnumerable<Warehouses>? Warehouses => Content["warehouse"] as IEnumerable<Warehouses>;
    private double _qty
    {
        get => DataResult.Qty;
        set
        {
            DataResult.Qty = value;
            DataResult.QtyLost = DataResult.QtyRequire - DataResult.QtyPlan - value;
        }
    }
    private IEnumerable<GetProductionOrder>? _selectedProductionOrder => Content["docNumOrderSelected"] as IEnumerable<GetProductionOrder>;
    private List<BatchReturnComponentProduction> _batchReceiptPo = new();
    private List<SerialReturnComponentProduction> _serialReceiptPo = new();
    public List<ItemNoneReturnComponentProduction> _productionOrderNumber { get; set; } = new();
    private bool IsItemBatch;
    private bool IsItemSerial;
    private bool IsItemNone;
    private IEnumerable<GetBatchOrSerial> _serialBatchDeliveryOrders = new List<GetBatchOrSerial>();
    
    private IEnumerable<ItemType> _type = Array.Empty<ItemType>();
    private Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> GetSerialBatch => Content["getSerialBatch"] as Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> ?? default!;
    private IEnumerable<GetProductionOrderLines> _selectedItem = Array.Empty<GetProductionOrderLines>();
    private IEnumerable<GetProductionOrderLines> ListGetProductionOrderLines => Content["item"] as IEnumerable<GetProductionOrderLines> ?? new List<GetProductionOrderLines>();

    string? dataGrid = "width: 1600px;";
    bool displayNoneOrShow;

    protected override async void OnInitialized()
    {
        _type = new List<ItemType>
        {
            new ItemType
            {
                Id=1,
                Name="Auto"
            },
            new ItemType
            {
                Id=2,
                Name="Manual"
            }
        };
        if (Content.TryGetValue("line", out var value))
        {
            Console.WriteLine(JsonSerializer.Serialize(value));
            DataResult = value as ReturnComponentProductionLine ?? new ReturnComponentProductionLine();
            _batchReceiptPo = DataResult.Batches ?? new List<BatchReturnComponentProduction>();
            Console.WriteLine(JsonSerializer.Serialize(_batchReceiptPo.FirstOrDefault()?.OnSelectedType));
            _serialReceiptPo = DataResult.Serials ?? new List<SerialReturnComponentProduction>();
            _productionOrderNumber = DataResult.ItemNones ?? new List<ItemNoneReturnComponentProduction>();
            _selectedItem = ListGetProductionOrderLines.Where(i => i.ItemCode == DataResult.ItemCode);
            if(_batchReceiptPo.Any(x => x.OnSelectedType.Count() != 0))
            {
                displayNoneOrShow = true;
            }
            await UpdateItemDetails(DataResult.ItemCode);
        }
    }

    private void OnSearch(OptionsSearchEventArgs<GetProductionOrderLines> e)
    {
        e.Items = ListGetProductionOrderLines?.Where(i => i.ItemCode.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                                                          i.ItemName.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.ItemCode);
    }

    private async Task SaveAsync()
    {
        DataResult.Batches = _batchReceiptPo;
        DataResult.Serials = _serialReceiptPo;
        DataResult.ItemNones = _productionOrderNumber;
        Console.WriteLine(JsonSerializer.Serialize(DataResult));
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

        Console.WriteLine(JsonSerializer.Serialize(firstItem));
        DataResult.ItemCode = firstItem?.ItemCode ?? "";
        DataResult.ItemName = firstItem?.ItemName ?? "";
        DataResult.WhsCode = firstItem?.WarehouseCode ?? "";
        DataResult.UomName = firstItem?.Uom ?? "";
        DataResult.ManageItem = firstItem?.ItemType;
        DataResult.QtyRequire = Convert.ToDouble(firstItem?.Qty ?? "0");
        DataResult.QtyPlan = Convert.ToDouble(firstItem?.PlanQty ?? "0");
        IsItemBatch = firstItem?.ItemType == "B";
        IsItemSerial = firstItem?.ItemType == "S";
        IsItemNone = firstItem?.ItemType == "N";
        //Console.WriteLine(JsonSerializer.Serialize(firstItem));
        if (firstItem?.ItemType != "N")
            _serialBatchDeliveryOrders = await GetSerialBatch(new Dictionary<string, string>
            {
                {"ItemCode", firstItem?.ItemCode ??""},
                {"ItemType", firstItem?.ItemType ??""},
                {"DocEntry", firstItem?.DocEntry ??""},
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
            _batchReceiptPo.Add(new BatchReturnComponentProduction { BatchCode = "", });
        }
        else if (IsItemSerial && _serialReceiptPo.Count() < DataResult.Qty)
        {
            _serialReceiptPo.Add(new SerialReturnComponentProduction { SerialCode = "", });
        }else if (IsItemNone)
        {
            _productionOrderNumber.Add(new ItemNoneReturnComponentProduction());
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
        else
        {
            _productionOrderNumber.RemoveAt(index);
        }
    }
    private void OnSearchSerial(OptionsSearchEventArgs<GetBatchOrSerial> e)
    {
        e.Items = _serialBatchDeliveryOrders?.Where(i => i.SerialBatch.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                                                         i.SerialBatch.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.SerialBatch);
    }
    private void OnSearchType(OptionsSearchEventArgs<ItemType> e)
    {
        e.Items = _type?.Where(i => i.Name.Contains(e.Text, StringComparison.OrdinalIgnoreCase));
    }
    private void OnSearchDocNum(OptionsSearchEventArgs<GetProductionOrder> e)
    {
        e.Items = _selectedProductionOrder?.Where(i => i.DocNum.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.DocNum);
    }
    private void OnSelectedType(string newValue)
    {
        Console.WriteLine("OnSelectedType");
        if (_batchReceiptPo.Where(x => x.OnSelectedType.Where(z => z.Name == "Manual").Count() != 0).Count() == 0)
        {
            displayNoneOrShow = false;
        }
        else
        {
            displayNoneOrShow = true;
        }
        StateHasChanged();
    }
    private void OnSelectedSerialOrBatch(string newValue, int index, string type)
    {

        if (type == "Batch")
        {
            var firstItem = _batchReceiptPo[index].OnSelectedBatchOrSerial.FirstOrDefault();
            //Console.WriteLine(firstItem);
            if (firstItem != null)
            {
                _batchReceiptPo[index].BatchCode = firstItem.SerialBatch ?? string.Empty;
                _batchReceiptPo[index].Qty = 0;
                _batchReceiptPo[index].QtyAvailable = Convert.ToDouble(firstItem.Qty);
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
    private void OnSelectedDocument(string newValue, int index)
    {
        //todo
        Console.WriteLine(index);
    }
    private void OnChangeQtyManual(ChangeEventArgs e, BatchReturnComponentProduction obj)
    {
        if (double.TryParse(e.Value?.ToString(), out double qty))
        {
            obj.QtyLost = qty;
            DataResult.QtyManual = _batchReceiptPo.Sum(x => x.QtyLost);
            DataResult.QtyLost = (DataResult.QtyRequire - DataResult.QtyPlan - DataResult.Qty) - _batchReceiptPo.Sum(x => x.QtyLost);
        }
    }
    private void OnChangeQtyManualItemNone(ChangeEventArgs e, ItemNoneReturnComponentProduction obj)
    {
        if (double.TryParse(e.Value?.ToString(), out double qty))
        {
            obj.QtyLost = qty;
            DataResult.QtyManual = _productionOrderNumber.Sum(x => x.QtyLost);
            DataResult.QtyLost = (DataResult.QtyRequire - DataResult.QtyPlan - DataResult.Qty) - _productionOrderNumber.Sum(x => x.QtyLost);
        }
    }
    //private void OnChangeQtyAuto(ChangeEventArgs e, BatchReturnComponentProduction obj)
    //{
    //    if (double.TryParse(e.Value?.ToString(), out double qty))
    //    { 
    //        DataResult.QtyLost = DataResult.QtyPlan - DataResult.QtyLost - qty;
    //        obj.Qty = qty;
    //    }
    //    Console.WriteLine(JsonSerializer.Serialize(DataResult));
    //}
}