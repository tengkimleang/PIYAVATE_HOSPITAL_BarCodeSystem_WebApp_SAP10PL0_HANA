using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.InventoryCounting;
using Piyavate_Hospital.Shared.Models.IssueForProduction;
using Piyavate_Hospital.Shared.Views.Shared.Component;

namespace Piyavate_Hospital.Shared.Views.IssueForProduction.MobileAppScreen.Add;

public partial class AddItemLineComponent
{
    [Parameter] public Func<Task> IsViewDetail { get; set; } = default!;
    [Parameter] public Dictionary<string, object> Content { get; set; } = default!;
    [Parameter] public Func<IssueProductionLine, Task> SaveItem { get; set; } = default!;

    [Inject] public IValidator<IssueProductionLine>? Validator { get; init; }
    [Inject] public IValidator<BatchIssueProduction>? ValidatorBatch { get; init; }
    [Inject] public IValidator<SerialIssueProduction>? ValidatorSerial { get; init; }
    private bool _isBackFromBatch = false;


    private IssueProductionLine DataResult { get; set; } = new();
    private List<BatchIssueProduction> _batchIssueProductions = new();
    private List<SerialIssueProduction> _serialIssueProductions = new();
    private IEnumerable<GetBatchOrSerial> _serialBatchInventoryCounting = new List<GetBatchOrSerial>();
    private bool _isItemBatch;
    private bool _isItemSerial;
    private IEnumerable<GetProductionOrderLines> _selectedItem = Array.Empty<GetProductionOrderLines>();
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

    private IEnumerable<GetProductionOrderLines> ListGetProductionOrderLines =>
        Content["item"] as IEnumerable<GetProductionOrderLines> ?? new List<GetProductionOrderLines>();

    // string? _dataGrid = "width: 1600px;overflow-x:scroll;";

    protected override async void OnInitialized()
    {
        if (Content.TryGetValue("line", out var value))
        {
            DataResult = value as IssueProductionLine ?? new IssueProductionLine();
            _batchIssueProductions = DataResult.Batches;
            _serialIssueProductions = DataResult.Serials;
            _selectedItem = ListGetProductionOrderLines.Where(i => i.ItemCode == DataResult.ItemCode);
            await UpdateItemDetails(DataResult.ItemCode);
        }
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
        DataResult.Batches = _batchIssueProductions;
        DataResult.Serials = _serialIssueProductions;
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
        DataResult.ManageItem = firstItem?.ItemType ?? "";
        DataResult.WhsCode = firstItem?.WarehouseCode ?? "";
        DataResult.QtyRequire = Convert.ToDouble(firstItem?.Qty ?? "0");
        DataResult.UomName = firstItem?.Uom ?? "";
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
            var firstItem = _batchIssueProductions?[index].OnSelectedBatchOrSerial.FirstOrDefault();
            if (firstItem != null)
            {
                if (_batchIssueProductions == null) return;
                _batchIssueProductions[index].BatchCode = firstItem.SerialBatch;
                _batchIssueProductions[index].Qty = 0;
                _batchIssueProductions[index].QtyAvailable = Convert.ToInt32(firstItem.Qty);
                _batchIssueProductions[index].ManfectureDate = DateTime.Parse(firstItem.MrfDate);
                _batchIssueProductions[index].ExpDate = DateTime.Parse(firstItem.ExpDate);
            }
        }
        else if (type == "Serial")
        {
            var firstItem = _serialIssueProductions?[index].OnSelectedBatchOrSerial.FirstOrDefault();
            if (firstItem != null)
            {
                if (_serialIssueProductions == null) return;
                _serialIssueProductions[index].SerialCode = firstItem.SerialBatch;
                _serialIssueProductions[index].Qty = 1;
                _serialIssueProductions[index].MfrDate = DateTime.Parse(firstItem.MrfDate);
                _serialIssueProductions[index].ExpDate = DateTime.Parse(firstItem.ExpDate);
                _serialIssueProductions[index].MfrNo = firstItem.MfrSerialNo;
            }
        }
    }

    private void AddLineToBatch(BatchIssueProduction? batchIssueProduction)
    {
        if (batchIssueProduction != null)
        {
            _getBatchOrSerials = new List<GetBatchOrSerial>
            {
                new GetBatchOrSerial(DataResult.ItemCode,
                    batchIssueProduction.QtyAvailable.ToString(CultureInfo.InvariantCulture),
                    batchIssueProduction.BatchCode, "",
                    batchIssueProduction.ExpDate.ToString() ?? "",
                    batchIssueProduction.ManfectureDate.ToString() ?? "",
                    "B", "",
                    batchIssueProduction.Qty.ToString(CultureInfo.InvariantCulture))
            };
            if (_batchIssueProductions != null)
                _indexOfLineBatch = _batchIssueProductions.IndexOf(batchIssueProduction);
            _isUpdate = true;
        }

        _isBackFromBatch = true;
        StateHasChanged();
    }

    private void AddLineToSerial(SerialIssueProduction? serialIssueProduction)
    {
        if (serialIssueProduction != null)
        {
            _getBatchOrSerials = new List<GetBatchOrSerial>
            {
                new GetBatchOrSerial(DataResult.ItemCode,
                    serialIssueProduction.Qty.ToString(CultureInfo.InvariantCulture),
                    serialIssueProduction.SerialCode, "",
                    serialIssueProduction.ExpDate.ToString() ?? "",
                    serialIssueProduction.MfrDate.ToString() ?? "",
                    "B", "",
                    serialIssueProduction.Qty.ToString(CultureInfo.InvariantCulture))
            };
            if (_serialIssueProductions != null)
                _indexOfLineSerial = _serialIssueProductions.IndexOf(serialIssueProduction);
            _isUpdate = true;
        }

        _isBackFromBatch = true;
        StateHasChanged();
    }

    private void DeleteLineFromBatchOrSerial(int index)
    {
        if (_isItemBatch)
        {
            _batchIssueProductions?.RemoveAt(index);
        }
        else if (_isItemSerial)
        {
            _serialIssueProductions?.RemoveAt(index);
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
        _batchIssueProductions?.RemoveAt(index);
        await OnAddItemLineBack();
    }

    private async Task OnAddBatchLine(BatchIssueProduction batchIssueProduction)
    {
        var result = await ValidatorBatch!.ValidateAsync(batchIssueProduction).ConfigureAwait(false);
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
            _batchIssueProductions?.Add(batchIssueProduction);
        }
        else
        {
            if (_batchIssueProductions != null) _batchIssueProductions[_indexOfLineBatch] = batchIssueProduction;
        }

        _isBackFromBatch = false;
        StateHasChanged();
    }

    private async Task OnAddSerialLine(SerialIssueProduction serialIssueProduction)
    {
        var result = await ValidatorSerial!.ValidateAsync(serialIssueProduction).ConfigureAwait(false);
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
            _serialIssueProductions?.Add(serialIssueProduction);
        }
        else
        {
            if (_serialIssueProductions != null) _serialIssueProductions[_indexOfLineSerial] = serialIssueProduction;
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
        // _batchIssueProductions.RemoveAt(index);
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
        _batchIssueProductions?.RemoveAt(Convert.ToInt32(dictionary["Index"]));
        _isBackFromBatch = false;
        FluentToast fluentToast = (FluentToast)dictionary["FluentToast"];
        fluentToast.Close();
        OnAddItemLineBack();
        return Task.CompletedTask;
    }

    private Task OnDeleteSerial(Dictionary<string, object> dictionary)
    {
        _serialIssueProductions?.RemoveAt(Convert.ToInt32(dictionary["Index"]));
        _isBackFromBatch = false;
        FluentToast fluentToast = (FluentToast)dictionary["FluentToast"];
        fluentToast.Close();
        OnAddItemLineBack();
        return Task.CompletedTask;
    }

    private async Task OnConfirmLine()
    {
        DataResult.Batches = _batchIssueProductions;
        DataResult.Serials = _serialIssueProductions;
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