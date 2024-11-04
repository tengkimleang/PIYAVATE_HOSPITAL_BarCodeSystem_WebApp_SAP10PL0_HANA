using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.GoodReceiptPo;
using Piyavate_Hospital.Shared.Views.Shared.Component;

namespace Piyavate_Hospital.Shared.Views.GoodReceptPo.MobileAppScreen.Add;

public partial class AddItemLineComponent
{
    [Parameter] public Func<Task> IsViewDetail { get; set; } = default!;
    [Parameter] public Dictionary<string, object> Content { get; set; } = default!;
    [Parameter] public Func<GoodReceiptPoLine, Task> SaveItem { get; set; } = default!;
    [Parameter] public Func<Dictionary<string, object>, Task<string>> GetGenerateBatchSerial { get; set; } = default!;
    [Inject] public IValidator<GoodReceiptPoLine>? Validator { get; init; }
    [Inject] public IValidator<BatchReceiptPo>? ValidatorBatch { get; init; }
    [Inject] public IValidator<SerialReceiptPo>? ValidatorSerial { get; init; }
    private bool _isBackFromBatch = false;


    private GoodReceiptPoLine DataResult { get; set; } = new();
    private List<BatchReceiptPo> _batchReceiptPo = new();
    private List<SerialReceiptPo> _serialReceiptPo = new();
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
    // private Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> GetSerialBatch =>
    //     Content["getSerialBatch"] as Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> ??
    //     default!;

    private IEnumerable<VatGroups>? VatGroup => Content["taxPurchase"] as IEnumerable<VatGroups>;
    private IEnumerable<Warehouses>? Warehouses => Content["warehouse"] as IEnumerable<Warehouses>;
    // string? _dataGrid = "width: 1600px;overflow-x:scroll;";

    protected override async void OnInitialized()
    {
        if (Content.TryGetValue("line", out var value))
        {
            DataResult = value as GoodReceiptPoLine ?? new GoodReceiptPoLine();
            _batchReceiptPo = DataResult.Batches ?? new List<BatchReceiptPo>();
            _serialReceiptPo = DataResult.Serials ?? new List<SerialReceiptPo>();
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

    private Task UpdateItemDetails(string newValue)
    {
        var firstItem = _selectedItem.FirstOrDefault();
        DataResult.Price = (DataResult.Price == 0) ? double.Parse(firstItem?.PriceUnit ?? "0") : DataResult.Price;
        DataResult.ItemCode = firstItem?.ItemCode ?? "";
        DataResult.ItemName = firstItem?.ItemName ?? "";
        DataResult.ManageItem = firstItem?.ItemType;
        _isItemBatch = firstItem?.ItemType == "B";
        _isItemSerial = firstItem?.ItemType == "S";
        // if (firstItem?.ItemType != "N")
        //     _serialBatchDeliveryOrders = await GetSerialBatch(new Dictionary<string, string>
        //     {
        //         { "ItemCode", firstItem?.ItemCode ?? "" },
        //         { "ItemType", firstItem?.ItemType ?? "" }
        //     });
        StateHasChanged();
        return Task.CompletedTask;
    }

    private void OnSelectedSerialOrBatch(string newValue, int index, string type)
    {
        if (type == "Batch")
        {
            var firstItem = _batchReceiptPo[index];
            _batchReceiptPo[index].BatchCode = firstItem.BatchCode;
            _batchReceiptPo[index].Qty = 0;
            _batchReceiptPo[index].ManfectureDate = firstItem.ManfectureDate;
            _batchReceiptPo[index].ExpDate = firstItem.ExpDate;
        }
        else if (type == "Serial")
        {
            var firstItem = _serialReceiptPo[index];
            _serialReceiptPo[index].SerialCode = firstItem.SerialCode;
            _serialReceiptPo[index].Qty = 1;
            _serialReceiptPo[index].MfrDate = firstItem.MfrDate;
            _serialReceiptPo[index].ExpDate = firstItem.ExpDate;
            _serialReceiptPo[index].MfrNo = firstItem.MfrNo;
        }
    }

    private void AddLineToBatch(BatchReceiptPo? batchDeliveryOrder)
    {
        if (batchDeliveryOrder != null)
        {
            _getBatchOrSerials = new List<GetBatchOrSerial>
            {
                new GetBatchOrSerial(DataResult.ItemCode,
                    batchDeliveryOrder.Qty.ToString(CultureInfo.InvariantCulture) ?? "0", batchDeliveryOrder.BatchCode,
                    "",
                    batchDeliveryOrder.ExpDate.ToString() ?? "",
                    batchDeliveryOrder.ManfectureDate.ToString() ?? "",
                    batchDeliveryOrder.ManfectureDate.ToString() ?? ""
                    , _batchReceiptPo.IndexOf(batchDeliveryOrder).ToString() ?? "0",
                    batchDeliveryOrder.Qty.ToString(CultureInfo.InvariantCulture),
                    AdmissionDate: batchDeliveryOrder.AdmissionDate.ToString() ?? "")
            };
            _indexOfLineBatch = _batchReceiptPo.IndexOf(batchDeliveryOrder);
            _isUpdate = true;
        }

        _isBackFromBatch = true;
        StateHasChanged();
    }

    private void AddLineToSerial(SerialReceiptPo? serialDeliveryOrder)
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
            NavigationManager.NavigateTo("deliveryorder");
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

    private async Task OnAddBatchLine(BatchReceiptPo batchReceiptPo)
    {
        var result = await ValidatorBatch!.ValidateAsync(batchReceiptPo).ConfigureAwait(false);
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
            _batchReceiptPo.Add(batchReceiptPo);
        }
        else
        {
            _batchReceiptPo[_indexOfLineBatch] = batchReceiptPo;
        }

        _isBackFromBatch = false;
        StateHasChanged();
    }

    private async Task OnAddSerialLine(SerialReceiptPo serialReceiptPo)
    {
        var result = await ValidatorSerial!.ValidateAsync(serialReceiptPo).ConfigureAwait(false);
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
            _serialReceiptPo.Add(serialReceiptPo);
        }
        else
        {
            _serialReceiptPo[_indexOfLineSerial] = serialReceiptPo;
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

    private async Task<string> OnGetGenerateBatchSerial()
    {
        var data = new Dictionary<string, object>
        {
            { "itemCode", DataResult.ItemCode },
            { "qty", DataResult.Qty ?? 0 }
        };
        return await GetGenerateBatchSerial(data);
    }
}