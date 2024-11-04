using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.InventoryCounting;
using Piyavate_Hospital.Shared.Views.Shared.Component;

namespace Piyavate_Hospital.Shared.Views.InventoryCounting.MobileAppScreen.Add;

public partial class AddItemLineComponent
{
    [Parameter] public Func<Task> IsViewDetail { get; set; } = default!;
    [Parameter] public Dictionary<string, object> Content { get; set; } = default!;
    [Parameter] public Func<InventoryCountingLine, Task> SaveItem { get; set; } = default!;

    [Inject] public IValidator<InventoryCountingLine>? Validator { get; init; }
    [Inject] public IValidator<InventoryCountingBatch>? ValidatorBatch { get; init; }
    [Inject] public IValidator<InventoryCountingSerial>? ValidatorSerial { get; init; }
    private bool _isBackFromBatch = false;


    private InventoryCountingLine DataResult { get; set; } = new();
    private List<InventoryCountingBatch> _inventoryCountingBatches = new();
    private List<InventoryCountingSerial> _inventoryCountingSerials = new();
    private IEnumerable<GetBatchOrSerial> _serialBatchInventoryCounting = new List<GetBatchOrSerial>();
    private bool _isItemBatch;
    private bool _isItemSerial;
    private IEnumerable<GetInventoryCountingLines> _selectedItem = Array.Empty<GetInventoryCountingLines>();
    private IEnumerable<Warehouses>? _selectedWarehouses = Array.Empty<Warehouses>();
    private IEnumerable<GetBatchOrSerial> _selectedSerialDeliveryOrders = Array.Empty<GetBatchOrSerial>();
    private bool _isUpdate = false;
    private int _indexOfLineBatch = 0;
    private int _indexOfLineSerial = 0;
    private IEnumerable<GetBatchOrSerial> _getBatchOrSerials = Array.Empty<GetBatchOrSerial>();
    private Func<int, Task> OnDeleteLineItem => Content["OnDeleteLineItem"] as Func<int, Task> ?? default!;

    private Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> GetSerialBatch =>
        Content["getSerialBatch"] as Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> ??
        default!;

    private IEnumerable<GetInventoryCountingLines> ListGetProductionOrderLines =>
        Content["item"] as IEnumerable<GetInventoryCountingLines> ?? new List<GetInventoryCountingLines>();

    // string? _dataGrid = "width: 1600px;overflow-x:scroll;";

    protected override async void OnInitialized()
    {
        if (Content.TryGetValue("line", out var value))
        {
            DataResult = value as InventoryCountingLine ?? new InventoryCountingLine();
            _inventoryCountingBatches = DataResult.Batches;
            _inventoryCountingSerials = DataResult.Serials;
            _selectedItem = ListGetProductionOrderLines.Where(i => i.ItemCode == DataResult.ItemCode);
            await UpdateItemDetails(DataResult.ItemCode);
        }
    }

    private void OnSearch(OptionsSearchEventArgs<GetInventoryCountingLines> e)
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
        DataResult.Batches = _inventoryCountingBatches;
        DataResult.Serials = _inventoryCountingSerials;
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
        DataResult.ItemCode = firstItem?.ItemCode ?? "";
        DataResult.ItemName = firstItem?.ItemName ?? "";
        DataResult.ManageItem = firstItem?.ItemType ?? "";
        DataResult.WhsCode = firstItem?.WarehouseCode ?? "";
        DataResult.Qty = Convert.ToDouble(firstItem?.Qty ?? "0");
        DataResult.Uom = firstItem?.Uom ?? "";
        _isItemBatch = firstItem?.ItemType == "B";
        _isItemSerial = firstItem?.ItemType == "S";
        if (firstItem?.ItemType != "N")
            _serialBatchInventoryCounting = await GetSerialBatch(new Dictionary<string, string>
            {
                { "ItemCode", firstItem?.ItemCode ?? "" },
                { "ItemType", firstItem?.ItemType ?? "" }
            });
        StateHasChanged();
    }

    private void OnSelectedSerialOrBatch(string newValue, int index, string type)
    {
        if (type == "Batch")
        {
            var firstItem = _inventoryCountingBatches[index].OnSelectedBatchOrSerial.FirstOrDefault();
            if (firstItem != null)
            {
                _inventoryCountingBatches[index].BatchCode = firstItem.SerialBatch;
                _inventoryCountingBatches[index].Qty = 0;
                _inventoryCountingBatches[index].QtyAvailable = Convert.ToInt32(firstItem.Qty);
                _inventoryCountingBatches[index].ManufactureDate = DateTime.Parse(firstItem.MrfDate);
                _inventoryCountingBatches[index].ExpireDate = DateTime.Parse(firstItem.ExpDate);
            }
        }
        else if (type == "Serial")
        {
            var firstItem = _inventoryCountingSerials[index].OnSelectedBatchOrSerial.FirstOrDefault();
            if (firstItem != null)
            {
                _inventoryCountingSerials[index].SerialCode = firstItem.SerialBatch;
                _inventoryCountingSerials[index].Qty = 1;
                _inventoryCountingSerials[index].MfrDate = DateTime.Parse(firstItem.MrfDate);
                _inventoryCountingSerials[index].ExpDate = DateTime.Parse(firstItem.ExpDate);
                _inventoryCountingSerials[index].MfrNo = firstItem.MfrSerialNo;
            }
        }
    }

    private void AddLineToBatch(InventoryCountingBatch? inventoryCountingBatch)
    {
        if (inventoryCountingBatch != null)
        {
            _getBatchOrSerials = new List<GetBatchOrSerial>
            {
                new GetBatchOrSerial(DataResult.ItemCode,
                    inventoryCountingBatch.QtyAvailable.ToString(CultureInfo.InvariantCulture),
                    inventoryCountingBatch.BatchCode, "",
                    inventoryCountingBatch.ExpireDate.ToString() ?? "",
                    inventoryCountingBatch.ManufactureDate.ToString() ?? "",
                    "B", "",
                    inventoryCountingBatch.Qty.ToString(CultureInfo.InvariantCulture))
            };
            _indexOfLineBatch = _inventoryCountingBatches.IndexOf(inventoryCountingBatch);
            _isUpdate = true;
        }

        _isBackFromBatch = true;
        StateHasChanged();
    }

    private void AddLineToSerial(InventoryCountingSerial? inventoryCountingSerial)
    {
        if (inventoryCountingSerial != null)
        {
            _getBatchOrSerials = new List<GetBatchOrSerial>
            {
                new GetBatchOrSerial(DataResult.ItemCode,
                    inventoryCountingSerial.Qty.ToString(CultureInfo.InvariantCulture),
                    inventoryCountingSerial.SerialCode, "",
                    inventoryCountingSerial.ExpDate.ToString() ?? "",
                    inventoryCountingSerial.MfrDate.ToString() ?? "",
                    "B", "",
                    inventoryCountingSerial.Qty.ToString(CultureInfo.InvariantCulture))
            };
            _indexOfLineSerial = _inventoryCountingSerials.IndexOf(inventoryCountingSerial);
            _isUpdate = true;
        }

        _isBackFromBatch = true;
        StateHasChanged();
    }

    private void DeleteLineFromBatchOrSerial(int index)
    {
        if (_isItemBatch)
        {
            _inventoryCountingBatches.RemoveAt(index);
        }
        else if (_isItemSerial)
        {
            _inventoryCountingSerials.RemoveAt(index);
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
        _inventoryCountingBatches.RemoveAt(index);
        await OnAddItemLineBack();
    }

    private async Task OnAddBatchLine(InventoryCountingBatch inventoryCountingBatch)
    {
        Console.WriteLine(JsonSerializer.Serialize(inventoryCountingBatch));
        var result = await ValidatorBatch!.ValidateAsync(inventoryCountingBatch).ConfigureAwait(false);
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
            _inventoryCountingBatches.Add(inventoryCountingBatch);
        }
        else
        {
            _inventoryCountingBatches[_indexOfLineBatch] = inventoryCountingBatch;
        }

        _isBackFromBatch = false;
        StateHasChanged();
    }

    private async Task OnAddSerialLine(InventoryCountingSerial inventoryCountingSerial)
    {
        var result = await ValidatorSerial!.ValidateAsync(inventoryCountingSerial).ConfigureAwait(false);
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
            _inventoryCountingSerials.Add(inventoryCountingSerial);
        }
        else
        {
            _inventoryCountingSerials[_indexOfLineSerial] = inventoryCountingSerial;
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
        // _inventoryCountingBatches.RemoveAt(index);
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
        _inventoryCountingBatches.RemoveAt(Convert.ToInt32(dictionary["Index"]));
        _isBackFromBatch = false;
        FluentToast fluentToast = (FluentToast)dictionary["FluentToast"];
        fluentToast.Close();
        OnAddItemLineBack();
        return Task.CompletedTask;
    }

    private Task OnDeleteSerial(Dictionary<string, object> dictionary)
    {
        _inventoryCountingSerials.RemoveAt(Convert.ToInt32(dictionary["Index"]));
        _isBackFromBatch = false;
        FluentToast fluentToast = (FluentToast)dictionary["FluentToast"];
        fluentToast.Close();
        OnAddItemLineBack();
        return Task.CompletedTask;
    }

    private async Task OnConfirmLine()
    {
        _inventoryCountingBatches.ForEach(x=>x.ItemCode = DataResult.ItemCode);
        _inventoryCountingSerials.ForEach(x=>x.ItemCode = DataResult.ItemCode);
        DataResult.Batches = _inventoryCountingBatches;
        DataResult.Serials = _inventoryCountingSerials;
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
}