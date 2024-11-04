using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.ReturnComponentProduction;
using Piyavate_Hospital.Shared.Views.Shared.Component;

namespace Piyavate_Hospital.Shared.Views.ReturnComponent.MobileAppScreen.Add;

public partial class AddItemLineComponent
{
    [Parameter] public Func<Task> IsViewDetail { get; set; } = default!;
    [Parameter] public Dictionary<string, object> Content { get; set; } = default!;
    [Parameter] public Func<ReturnComponentProductionLine, Task> SaveItem { get; set; } = default!;

    [Inject] public IValidator<ReturnComponentProductionLine>? Validator { get; init; }
    [Inject] public IValidator<BatchReturnComponentProduction>? ValidatorBatch { get; init; }
    [Inject] public IValidator<SerialReturnComponentProduction>? ValidatorSerial { get; init; }
    private bool _isBackFromBatch = false;


    private ReturnComponentProductionLine DataResult { get; set; } = new();
    private List<BatchReturnComponentProduction> _batchReturnComponent = new();
    private List<SerialReturnComponentProduction> _serialReturnComponent = new();
    private IEnumerable<GetBatchOrSerial> _serialBatchInventoryCounting = new List<GetBatchOrSerial>();
    private IEnumerable<GetProductionOrderLines> _selectedItem = Array.Empty<GetProductionOrderLines>();
    private IEnumerable<Warehouses>? _selectedWarehouses = Array.Empty<Warehouses>();
    private IEnumerable<GetBatchOrSerial> _selectedSerialDeliveryOrders = Array.Empty<GetBatchOrSerial>();
    private bool _isUpdate = false;
    private int _indexOfLineBatch = 0;
    private int _indexOfLineSerial = 0;
    private bool _isItemBatch;
    private bool _isItemSerial;
    private bool _isItemNone;
    private List<ItemNoneReturnComponentProduction> ProductionOrderNumber { get; set; } = new();
    private IEnumerable<GetBatchOrSerial> _serialBatchDeliveryOrders = new List<GetBatchOrSerial>();
    private IEnumerable<GetBatchOrSerial> _getBatchOrSerials = Array.Empty<GetBatchOrSerial>();

    private IEnumerable<ItemNoneReturnComponentProduction> _itemNoneReturnComponentProductions =
        Array.Empty<ItemNoneReturnComponentProduction>();

    private Func<int, Task> OnDeleteLineItem => Content["OnDeleteLineItem"] as Func<int, Task> ?? default!;

    private IEnumerable<GetProductionOrder> SelectedProductionOrder =>
        Content["docNumOrderSelected"] as IEnumerable<GetProductionOrder> ?? default!;

    private Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> GetSerialBatch =>
        Content["getSerialBatch"] as Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> ??
        default!;

    private double Qty
    {
        get => DataResult.Qty;
        set
        {
            DataResult.Qty = value;
            DataResult.QtyLost = DataResult.QtyRequire - DataResult.QtyPlan - value;
        }
    }

    private IEnumerable<GetProductionOrderLines> ListGetProductionOrderLines =>
        Content["item"] as IEnumerable<GetProductionOrderLines> ?? new List<GetProductionOrderLines>();

    // string? _dataGrid = "width: 1600px;overflow-x:scroll;";

    protected override async void OnInitialized()
    {
        if (!Content.TryGetValue("line", out var value)) return;
        DataResult = value as ReturnComponentProductionLine ?? new ReturnComponentProductionLine();
        if (string.IsNullOrEmpty(DataResult.ItemCode)) return;
        Console.WriteLine("Hello");
        _batchReturnComponent = DataResult.Batches;
        _selectedWarehouses = Warehouses?.Where(i => i.Code == DataResult.WhsCode);
        _serialReturnComponent = DataResult.Serials;
        _selectedItem = ListGetProductionOrderLines.Where(i => i.ItemCode == DataResult.ItemCode);
        await UpdateItemDetails(DataResult.ItemCode);
    }

    private void OnSearch(OptionsSearchEventArgs<GetProductionOrderLines> e)
    {
        e.Items = ListGetProductionOrderLines.Where(i =>
                i.ItemCode.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                i.ItemName.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.ItemCode);
    }

    private void OnSearchSerial(OptionsSearchEventArgs<GetBatchOrSerial> e)
    {
        e.Items = _serialBatchInventoryCounting.Where(i =>
                i.SerialBatch.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                i.SerialBatch.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.SerialBatch);
    }

    private async Task SaveAsync()
    {
        DataResult.Batches = _batchReturnComponent;
        DataResult.Serials = _serialReturnComponent;
        var result = await Validator!.ValidateAsync(DataResult).ConfigureAwait(false);
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                ToastService!.ShowError(error.ErrorMessage);
            }
        }
        //To Do: Save Data
        // await Dialog.CloseAsync(new Dictionary<string, object>
        // {
        //     { "data", DataResult },
        //     { "isUpdate", Content.ContainsKey("line") }
        // });
    }

    private Task UpdateWarehouses(string newValue)
    {
        Console.WriteLine(newValue);
        if (_selectedWarehouses != null)
        {
            var firstItem = _selectedWarehouses.FirstOrDefault();
            if (firstItem != null)
                DataResult.WhsCode = firstItem.Code;
        }

        return Task.CompletedTask;
    }

    private async Task UpdateItemDetails(string newValue)
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
        _isItemBatch = firstItem?.ItemType == "B";
        _isItemSerial = firstItem?.ItemType == "S";
        _isItemNone = firstItem?.ItemType == "N";
        Console.WriteLine(JsonSerializer.Serialize(firstItem));
        if (firstItem?.ItemType != "N")
            _serialBatchDeliveryOrders = await GetSerialBatch(new Dictionary<string, string>
            {
                { "ItemCode", firstItem?.ItemCode ?? "" },
                { "ItemType", firstItem?.ItemType ?? "" },
                { "DocEntry", firstItem?.DocEntry ?? "" },
            });
    }

    private void OnSelectedSerialOrBatch(string newValue, int index, string type)
    {
        if (type == "Batch")
        {
            var firstItem = _batchReturnComponent?[index].OnSelectedBatchOrSerial.FirstOrDefault();
            if (firstItem != null)
            {
                if (_batchReturnComponent == null) return;
                _batchReturnComponent[index].BatchCode = firstItem.SerialBatch;
                _batchReturnComponent[index].Qty = 0;
                _batchReturnComponent[index].QtyAvailable = Convert.ToInt32(firstItem.Qty);
                _batchReturnComponent[index].ManfectureDate = DateTime.Parse(firstItem.MrfDate);
                _batchReturnComponent[index].ExpDate = DateTime.Parse(firstItem.ExpDate);
            }
        }
        else if (type == "Serial")
        {
            var firstItem = _serialReturnComponent?[index].OnSelectedBatchOrSerial.FirstOrDefault();
            if (firstItem != null)
            {
                if (_serialReturnComponent == null) return;
                _serialReturnComponent[index].SerialCode = firstItem.SerialBatch;
                _serialReturnComponent[index].Qty = 1;
                _serialReturnComponent[index].MfrDate = DateTime.Parse(firstItem.MrfDate);
                _serialReturnComponent[index].ExpDate = DateTime.Parse(firstItem.ExpDate);
                _serialReturnComponent[index].MfrNo = firstItem.MfrSerialNo;
            }
        }
    }

    private void AddLineToNone(ItemNoneReturnComponentProduction? itemNoneReturnComponentProduction)
    {
        //To Do: Add Line to None
        // if (itemNoneReturnComponentProduction != null)
        // {
        //     _itemNoneReturnComponentProductions = new List<ItemNoneReturnComponentProduction>
        //     {
        //         new ItemNoneReturnComponentProduction(
        //             itemNoneReturnComponentProduction.OnSelectedProductionOrder.FirstOrDefault().DocNum
        //             )
        //     };
        //     if (_batchReturnComponent != null)
        //         _indexOfLineBatch = _batchReturnComponent.IndexOf(itemNoneReturnComponentProduction);
        //     _isUpdate = true;
        // }

        _isBackFromBatch = true;
        StateHasChanged();
    }

    private void AddLineToBatch(BatchReturnComponentProduction? batchReturnComponentProduction)
    {
        if (batchReturnComponentProduction != null)
        {
            _getBatchOrSerials = new List<GetBatchOrSerial>
            {
                new GetBatchOrSerial(DataResult.ItemCode,
                    batchReturnComponentProduction.QtyAvailable.ToString(CultureInfo.InvariantCulture),
                    batchReturnComponentProduction.BatchCode, "",
                    batchReturnComponentProduction.ExpDate.ToString() ?? "",
                    batchReturnComponentProduction.ManfectureDate.ToString() ?? "",
                    "B", "",
                    batchReturnComponentProduction.Qty.ToString(CultureInfo.InvariantCulture))
            };
            if (_batchReturnComponent != null)
                _indexOfLineBatch = _batchReturnComponent.IndexOf(batchReturnComponentProduction);
            _isUpdate = true;
        }

        _isBackFromBatch = true;
        StateHasChanged();
    }

    private void AddLineToSerial(SerialReturnComponentProduction? serialReturnComponentProduction)
    {
        if (serialReturnComponentProduction != null)
        {
            _getBatchOrSerials = new List<GetBatchOrSerial>
            {
                new GetBatchOrSerial(DataResult.ItemCode,
                    serialReturnComponentProduction.Qty.ToString(CultureInfo.InvariantCulture),
                    serialReturnComponentProduction.SerialCode, "",
                    serialReturnComponentProduction.ExpDate.ToString() ?? "",
                    serialReturnComponentProduction.MfrDate.ToString() ?? "",
                    "B", "",
                    serialReturnComponentProduction.Qty.ToString(CultureInfo.InvariantCulture))
            };
            if (_serialReturnComponent != null)
                _indexOfLineSerial = _serialReturnComponent.IndexOf(serialReturnComponentProduction);
            _isUpdate = true;
        }

        _isBackFromBatch = true;
        StateHasChanged();
    }

    private void DeleteLineFromBatchOrSerial(int index)
    {
        if (_isItemBatch)
        {
            _batchReturnComponent?.RemoveAt(index);
        }
        else if (_isItemSerial)
        {
            _serialReturnComponent?.RemoveAt(index);
        }
    }

    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("inventorycounting");
        }
    }

    private Task OnAddItemLineBack()
    {
        _isUpdate = false;
        _indexOfLineBatch = 0;
        _isBackFromBatch = false;
        _getBatchOrSerials = Array.Empty<GetBatchOrSerial>();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task OnDeleteItemLine(int index)
    {
        _batchReturnComponent?.RemoveAt(index);
        await OnAddItemLineBack();
    }

    private async Task OnAddBatchLine(BatchReturnComponentProduction batchReturnComponentProduction)
    {
        var result = await ValidatorBatch!.ValidateAsync(batchReturnComponentProduction).ConfigureAwait(false);
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                ToastService!.ShowError(error.ErrorMessage);
            }

            return;
        }

        Console.WriteLine("After Add Manual");
        Console.WriteLine(JsonSerializer.Serialize(batchReturnComponentProduction));
        Console.WriteLine(JsonSerializer.Serialize(DataResult));
        if (_isUpdate == false)
        {
            _batchReturnComponent?.Add(batchReturnComponentProduction);
        }
        else
        {
            if (_batchReturnComponent != null)
                _batchReturnComponent[_indexOfLineBatch] = batchReturnComponentProduction;
        }

        _isBackFromBatch = false;
        StateHasChanged();
    }

    private async Task OnAddSerialLine(SerialReturnComponentProduction serialReturnComponentProduction)
    {
        var result = await ValidatorSerial!.ValidateAsync(serialReturnComponentProduction).ConfigureAwait(false);
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                ToastService!.ShowError(error.ErrorMessage);
            }

            return;
        }

        if (_isUpdate == false)
        {
            _serialReturnComponent?.Add(serialReturnComponentProduction);
        }
        else
        {
            if (_serialReturnComponent != null)
                _serialReturnComponent[_indexOfLineSerial] = serialReturnComponentProduction;
        }

        _isBackFromBatch = false;
        StateHasChanged();
    }

    private Task OnDeleteBatchLine(int index)
    {
        ToastService.ShowToast<ToastCustom, Dictionary<string, object>>(
            new ToastParameters<Dictionary<string, object>>()
            {
                Intent = ToastIntent.Custom,
                Title = "Delete Batch",
                Timeout = 6000,
                Icon = (new Icons.Regular.Size20.Delete(), Color.Accent),
                Content = new Dictionary<string, object>
                {
                    {
                        "Body", "Are you sure to Delete Batch?"
                    },
                    {
                        "Index", index
                    },
                    {
                        "OnClickPrimaryButton", new Func<Dictionary<string, object>, Task>(OnDeleteBatch)
                    },
                    {
                        "PrimaryButtonText", "Confirm"
                    },
                    {
                        "ButtonPrimaryColor", "var(--bs-green)"
                    },
                    {
                        "ButtonSecondaryColor", "var(--bs-red)"
                    }
                }
            });
        // _batchReturnComponent.RemoveAt(index);
        // _isBackFromBatch = false;
        // StateHasChanged();
        return Task.CompletedTask;
    }

    private Task OnDeleteSerialLine(int index)
    {
        ToastService.ShowToast<ToastCustom, Dictionary<string, object>>(
            new ToastParameters<Dictionary<string, object>>()
            {
                Intent = ToastIntent.Custom,
                Title = "Delete Serial",
                Timeout = 6000,
                Icon = (new Icons.Regular.Size20.Delete(), Color.Accent),
                Content = new Dictionary<string, object>
                {
                    {
                        "Body", "Are you sure to Delete Serial?"
                    },
                    {
                        "Index", index
                    },
                    {
                        "OnClickPrimaryButton", new Func<Dictionary<string, object>, Task>(OnDeleteSerial)
                    },
                    {
                        "PrimaryButtonText", "Delete"
                    },
                    {
                        "ButtonPrimaryColor", "var(--bs-green)"
                    },
                    {
                        "ButtonSecondaryColor", "var(--bs-red)"
                    }
                }
            });
        return Task.CompletedTask;
    }

    private Task OnDeleteBatch(Dictionary<string, object> dictionary)
    {
        _batchReturnComponent?.RemoveAt(Convert.ToInt32(dictionary["Index"]));
        _isBackFromBatch = false;
        FluentToast fluentToast = (FluentToast)dictionary["FluentToast"];
        fluentToast.Close();
        OnAddItemLineBack();
        return Task.CompletedTask;
    }

    private Task OnDeleteSerial(Dictionary<string, object> dictionary)
    {
        _serialReturnComponent?.RemoveAt(Convert.ToInt32(dictionary["Index"]));
        _isBackFromBatch = false;
        FluentToast fluentToast = (FluentToast)dictionary["FluentToast"];
        fluentToast.Close();
        OnAddItemLineBack();
        return Task.CompletedTask;
    }

    private async Task OnConfirmLine()
    {
        DataResult.Batches = _batchReturnComponent;
        DataResult.Serials = _serialReturnComponent;
        var result = await Validator!.ValidateAsync(DataResult).ConfigureAwait(false);
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                ToastService!.ShowError(error.ErrorMessage);
            }

            return;
        }

        await SaveItem(DataResult);
    }

    private void OnSearchWarehouses(OptionsSearchEventArgs<Warehouses> e)
    {
        e.Items = Warehouses?.Where(i => i.Name.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                                         i.Code.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.Code);
    }

    private IEnumerable<Warehouses>? Warehouses => Content["warehouse"] as IEnumerable<Warehouses>;
}