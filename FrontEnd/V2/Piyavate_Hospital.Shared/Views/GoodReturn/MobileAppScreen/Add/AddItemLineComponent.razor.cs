using System.Collections.ObjectModel;
using System.Globalization;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Views.Shared.Component;

namespace Piyavate_Hospital.Shared.Views.GoodReturn.MobileAppScreen.Add;

public partial class AddItemLineComponent
{
    [Parameter] public Func<Task> IsViewDetail { get; set; } = default!;
    [Parameter] public Dictionary<string, object> Content { get; set; } = default!;
    [Parameter] public Func<DeliveryOrderLine, Task> SaveItem { get; set; } = default!;

    [Inject] public IValidator<DeliveryOrderLine>? Validator { get; init; }
    [Inject] public IValidator<BatchDeliveryOrder>? ValidatorBatch { get; init; }
    [Inject] public IValidator<SerialDeliveryOrder>? ValidatorSerial { get; init; }
    private bool _isBackFromBatch = false;


    private DeliveryOrderLine DataResult { get; set; } = new();
    private List<BatchDeliveryOrder> _batchReceiptPo = new();
    private List<SerialDeliveryOrder> _serialReceiptPo = new();
    private IEnumerable<GetBatchOrSerial> _serialBatchDeliveryOrders = new List<GetBatchOrSerial>();
    private bool _isItemBatch;
    private bool _isItemSerial;
    private IEnumerable<Items> _selectedItem = Array.Empty<Items>();
    private IEnumerable<Warehouses>? _selectedWarehouses = Array.Empty<Warehouses>();
    private IEnumerable<GetBatchOrSerial> _selectedSerialDeliveryOrders = Array.Empty<GetBatchOrSerial>();
    private bool _isUpdate = false;
    private int _indexOfLineBatch = 0;
    private int _indexOfLineSerial = 0;
    private IEnumerable<GetBatchOrSerial> _getBatchOrSerials = Array.Empty<GetBatchOrSerial>();
    private IEnumerable<Items> Item => Content["item"] as IEnumerable<Items> ?? new List<Items>();
    private Func<int, Task> OnDeleteLineItem => Content["OnDeleteLineItem"] as Func<int, Task> ?? default!;

    private Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> GetSerialBatch =>
        Content["getSerialBatch"] as Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> ??
        default!;

    private IEnumerable<VatGroups>? VatGroup => Content["taxPurchase"] as IEnumerable<VatGroups>;
    private IEnumerable<Warehouses>? Warehouses => Content["warehouse"] as IEnumerable<Warehouses>;
    // string? _dataGrid = "width: 1600px;overflow-x:scroll;";

    protected override async void OnInitialized()
    {
        if (Content.TryGetValue("line", out var value))
        {
            DataResult = value as DeliveryOrderLine ?? new DeliveryOrderLine();
            _batchReceiptPo = DataResult.Batches ?? new List<BatchDeliveryOrder>();
            _serialReceiptPo = DataResult.Serials ?? new List<SerialDeliveryOrder>();
            _selectedItem = Item.Where(i => i.ItemCode == DataResult.ItemCode);
            _selectedWarehouses = Warehouses?.Where(i => i.Code == DataResult.WarehouseCode);
            await UpdateItemDetails(DataResult.ItemCode);
            if (string.IsNullOrEmpty(DataResult.ItemCode))
                DataResult.VatCode = VatGroup?.FirstOrDefault()?.Code ?? "";
        }
    }

    private void OnSearch(OptionsSearchEventArgs<Items> e)
    {
        e.Items = Item.Where(i => i.ItemCode.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                                  i.ItemName.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.ItemCode);
    }

    private void OnSearchWarehouses(OptionsSearchEventArgs<Warehouses> e)
    {
        e.Items = Warehouses?.Where(i => i.Name.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                                         i.Code.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.Code);
    }

    private void OnSearchSerial(OptionsSearchEventArgs<GetBatchOrSerial> e)
    {
        e.Items = _serialBatchDeliveryOrders.Where(i =>
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
                DataResult.WarehouseCode = firstItem.Code;
        }

        return Task.CompletedTask;
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
            _serialBatchDeliveryOrders = await GetSerialBatch(new Dictionary<string, string>
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
            var firstItem = _batchReceiptPo[index].OnSelectedBatchOrSerial.FirstOrDefault();
            if (firstItem != null)
            {
                _batchReceiptPo[index].BatchCode = firstItem.SerialBatch;
                _batchReceiptPo[index].Qty = 0;
                _batchReceiptPo[index].QtyAvailable = Convert.ToInt32(firstItem.Qty);
                _batchReceiptPo[index].ManfectureDate = DateTime.Parse(firstItem.MrfDate);
                _batchReceiptPo[index].ExpDate = DateTime.Parse(firstItem.ExpDate);
            }
        }
        else if (type == "Serial")
        {
            var firstItem = _serialReceiptPo[index].OnSelectedBatchOrSerial.FirstOrDefault();
            if (firstItem != null)
            {
                _serialReceiptPo[index].SerialCode = firstItem.SerialBatch;
                _serialReceiptPo[index].Qty = 1;
                _serialReceiptPo[index].MfrDate = DateTime.Parse(firstItem.MrfDate);
                _serialReceiptPo[index].ExpDate = DateTime.Parse(firstItem.ExpDate);
                _serialReceiptPo[index].MfrNo = firstItem.MfrSerialNo;
            }
        }
    }

    private void AddLineToBatch(BatchDeliveryOrder? batchDeliveryOrder)
    {
        if (batchDeliveryOrder != null)
        {
            _getBatchOrSerials = new List<GetBatchOrSerial>
            {
                new GetBatchOrSerial(DataResult.ItemCode,
                    batchDeliveryOrder.QtyAvailable.ToString(CultureInfo.InvariantCulture),
                    batchDeliveryOrder.BatchCode, "",
                    batchDeliveryOrder.ExpDate.ToString() ?? "",
                    batchDeliveryOrder.ManfectureDate.ToString() ?? "",
                    "B", "",
                    batchDeliveryOrder.Qty.ToString(CultureInfo.InvariantCulture))
            };
            _indexOfLineBatch = _batchReceiptPo.IndexOf(batchDeliveryOrder);
            _isUpdate = true;
        }

        _isBackFromBatch = true;
        StateHasChanged();
    }

    private void AddLineToSerial(SerialDeliveryOrder? serialDeliveryOrder)
    {
        if (serialDeliveryOrder != null)
        {
            _getBatchOrSerials = new List<GetBatchOrSerial>
            {
                new GetBatchOrSerial(DataResult.ItemCode,
                    serialDeliveryOrder.Qty.ToString(CultureInfo.InvariantCulture),
                    serialDeliveryOrder.SerialCode, "",
                    serialDeliveryOrder.ExpDate.ToString() ?? "",
                    serialDeliveryOrder.MfrDate.ToString() ?? "",
                    "B", "",
                    serialDeliveryOrder.Qty.ToString(CultureInfo.InvariantCulture))
            };
            _indexOfLineSerial = _serialReceiptPo.IndexOf(serialDeliveryOrder);
            _isUpdate = true;
        }

        _isBackFromBatch = true;
        StateHasChanged();
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
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("return");
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
        _batchReceiptPo.RemoveAt(index);
        await OnAddItemLineBack();
    }

    private async Task OnAddBatchLine(BatchDeliveryOrder batchDeliveryOrder)
    {
        var result = await ValidatorBatch!.ValidateAsync(batchDeliveryOrder).ConfigureAwait(false);
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
            _batchReceiptPo.Add(batchDeliveryOrder);
        }
        else
        {
            _batchReceiptPo[_indexOfLineBatch] = batchDeliveryOrder;
        }

        _isBackFromBatch = false;
        StateHasChanged();
    }

    private async Task OnAddSerialLine(SerialDeliveryOrder serialDeliveryOrder)
    {
        var result = await ValidatorSerial!.ValidateAsync(serialDeliveryOrder).ConfigureAwait(false);
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
            _serialReceiptPo.Add(serialDeliveryOrder);
        }
        else
        {
            _serialReceiptPo[_indexOfLineSerial] = serialDeliveryOrder;
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
        // _batchReceiptPo.RemoveAt(index);
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
        _batchReceiptPo.RemoveAt(Convert.ToInt32(dictionary["Index"]));
        _isBackFromBatch = false;
        FluentToast fluentToast = (FluentToast)dictionary["FluentToast"];
        fluentToast.Close();
        OnAddItemLineBack();
        return Task.CompletedTask;
    }

    private Task OnDeleteSerial(Dictionary<string, object> dictionary)
    {
        _serialReceiptPo.RemoveAt(Convert.ToInt32(dictionary["Index"]));
        _isBackFromBatch = false;
        FluentToast fluentToast = (FluentToast)dictionary["FluentToast"];
        fluentToast.Close();
        OnAddItemLineBack();
        return Task.CompletedTask;
    }

    private async Task OnConfirmLine()
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

        await SaveItem(DataResult);
    }
}