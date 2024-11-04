using System.Globalization;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.ReceiptFinishGood;
using Piyavate_Hospital.Shared.Views.Shared.Component;

namespace Piyavate_Hospital.Shared.Views.ReceiptFromProduction.MobileAppScreen.Add;

public partial class AddItemLineComponent
{
    [Parameter] public Func<Task> IsViewDetail { get; set; } = default!;
    [Parameter] public Dictionary<string, object> Content { get; set; } = default!;
    [Parameter] public Func<ReceiptFinishGoodLine, Task> SaveItem { get; set; } = default!;
    [Parameter] public Func<Dictionary<string, object>, Task<string>> GetGenerateBatchSerial { get; set; } = default!;
    [Inject] public IValidator<ReceiptFinishGoodLine>? Validator { get; init; }
    [Inject] public IValidator<ReceiptFinishGoodBatch>? ValidatorBatch { get; init; }
    [Inject] public IValidator<ReceiptFinishGoodSerial>? ValidatorSerial { get; init; }
    private bool _isBackFromBatch = false;


    private ReceiptFinishGoodLine DataResult { get; set; } = new();
    private List<ReceiptFinishGoodBatch> _receiptFinishGoodBatches = new();
    private List<ReceiptFinishGoodSerial> _receiptFinishGoodSerials = new();
    private IEnumerable<GetBatchOrSerial> _serialBatchDeliveryOrders = new List<GetBatchOrSerial>();
    private bool _isItemBatch;
    private bool _isItemSerial;
    private IEnumerable<GetProductionOrderLines> _selectedItem = Array.Empty<GetProductionOrderLines>();
    private IEnumerable<Warehouses>? _selectedWarehouses = Array.Empty<Warehouses>();
    private IEnumerable<GetBatchOrSerial> _selectedSerialDeliveryOrders = Array.Empty<GetBatchOrSerial>();
    private bool _isUpdate = false;
    private int _indexOfLineBatch = 0;
    private int _indexOfLineSerial = 0;
    private IEnumerable<GetBatchOrSerial> _getBatchOrSerials = Array.Empty<GetBatchOrSerial>();
    private IEnumerable<GetProductionOrderLines> Items => Content["item"] as IEnumerable<GetProductionOrderLines> ??
                                                          new List<GetProductionOrderLines>();

    private Func<int, Task> OnDeleteLineItem => Content["OnDeleteLineItem"] as Func<int, Task> ?? default!;
    // private Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> GetSerialBatch =>
    //     Content["getSerialBatch"] as Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>> ??
    //     default!;

    private IEnumerable<VatGroups>? VatGroup => Content["taxPurchase"] as IEnumerable<VatGroups>;
    private IEnumerable<Warehouses>? Warehouses => Content["warehouse"] as IEnumerable<Warehouses>;
    // string? _dataGrid = "width: 1600px;overflow-x:scroll;";

    protected override void OnInitialized()
    {
        if (Content.TryGetValue("line", out var value))
        {
            DataResult = value as ReceiptFinishGoodLine ?? new ReceiptFinishGoodLine();
            _receiptFinishGoodBatches = DataResult.Batches;
            _receiptFinishGoodSerials = DataResult.Serials;
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
        DataResult.Batches = _receiptFinishGoodBatches;
        DataResult.Serials = _receiptFinishGoodSerials;
        var result = await Validator!.ValidateAsync(DataResult).ConfigureAwait(false);
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                ToastService!.ShowError(error.ErrorMessage);
            }
        }
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
        DataResult.DocNum = Convert.ToInt32(firstItem?.DocEntry ?? "0");
        _isItemBatch = firstItem?.ItemType == "B";
        _isItemSerial = firstItem?.ItemType == "S";
    }

    private void OnSelectedSerialOrBatch(string newValue, int index, string type)
    {
        if (type == "Batch")
        {
            var firstItem = _receiptFinishGoodBatches[index];
            _receiptFinishGoodBatches[index].BatchCode = firstItem.BatchCode;
            _receiptFinishGoodBatches[index].Qty = 0;
            _receiptFinishGoodBatches[index].ManufactureDate = firstItem.ManufactureDate;
            _receiptFinishGoodBatches[index].ExpDate = firstItem.ExpDate;
            _receiptFinishGoodBatches[index].AdmissionDate = firstItem.AdmissionDate;
        }
        else if (type == "Serial")
        {
            var firstItem = _receiptFinishGoodSerials[index];
            _receiptFinishGoodSerials[index].SerialCode = firstItem.SerialCode;
            _receiptFinishGoodSerials[index].Qty = 1;
            _receiptFinishGoodSerials[index].MfrDate = firstItem.MfrDate;
            _receiptFinishGoodSerials[index].ExpDate = firstItem.ExpDate;
            _receiptFinishGoodSerials[index].MfrNo = firstItem.MfrNo;
        }
    }

    private void AddLineToBatch(ReceiptFinishGoodBatch? receiptFinishGoodBatch)
    {
        if (receiptFinishGoodBatch != null)
        {
            _getBatchOrSerials = new List<GetBatchOrSerial>
            {
                new GetBatchOrSerial(DataResult.ItemCode,
                    receiptFinishGoodBatch.Qty.ToString(CultureInfo.InvariantCulture), receiptFinishGoodBatch.BatchCode,
                    "",
                    receiptFinishGoodBatch.ExpDate.ToString() ?? "",
                    receiptFinishGoodBatch.ManufactureDate.ToString() ?? "",
                    receiptFinishGoodBatch.ManufactureDate.ToString() ?? ""
                    , _receiptFinishGoodBatches.IndexOf(receiptFinishGoodBatch).ToString(),
                    receiptFinishGoodBatch.Qty.ToString(CultureInfo.InvariantCulture),
                    AdmissionDate: receiptFinishGoodBatch.AdmissionDate.ToString() ?? "")
            };
            _indexOfLineBatch = _receiptFinishGoodBatches.IndexOf(receiptFinishGoodBatch);
            _isUpdate = true;
        }

        _isBackFromBatch = true;
        StateHasChanged();
    }

    private void AddLineToSerial(ReceiptFinishGoodSerial? receiptFinishGoodSerial)
    {
        if (receiptFinishGoodSerial != null)
        {
            _getBatchOrSerials = new List<GetBatchOrSerial>
            {
                new GetBatchOrSerial(DataResult.ItemCode,
                    receiptFinishGoodSerial.Qty.ToString(CultureInfo.InvariantCulture),
                    receiptFinishGoodSerial.SerialCode, "",
                    receiptFinishGoodSerial.ExpDate.ToString() ?? "",
                    receiptFinishGoodSerial.MfrDate.ToString() ?? "",
                    "B", "",
                    receiptFinishGoodSerial.Qty.ToString(CultureInfo.InvariantCulture))
            };
            _indexOfLineSerial = _receiptFinishGoodSerials.IndexOf(receiptFinishGoodSerial);
            _isUpdate = true;
        }

        _isBackFromBatch = true;
        StateHasChanged();
    }

    private void DeleteLineFromBatchOrSerial(int index)
    {
        if (_isItemBatch)
        {
            _receiptFinishGoodBatches.RemoveAt(index);
        }
        else if (_isItemSerial)
        {
            _receiptFinishGoodSerials.RemoveAt(index);
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
        _receiptFinishGoodBatches.RemoveAt(index);
        await OnAddItemLineBack();
    }

    private async Task OnAddBatchLine(ReceiptFinishGoodBatch batch)
    {
        var result = await ValidatorBatch!.ValidateAsync(batch).ConfigureAwait(false);
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
            _receiptFinishGoodBatches.Add(batch);
        }
        else
        {
            _receiptFinishGoodBatches[_indexOfLineBatch] = batch;
        }

        _isBackFromBatch = false;
        StateHasChanged();
    }

    private async Task OnAddSerialLine(ReceiptFinishGoodSerial receiptFinishGoodSerial)
    {
        var result = await ValidatorSerial!.ValidateAsync(receiptFinishGoodSerial).ConfigureAwait(false);
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
            _receiptFinishGoodSerials.Add(receiptFinishGoodSerial);
        }
        else
        {
            _receiptFinishGoodSerials[_indexOfLineSerial] = receiptFinishGoodSerial;
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
        // _receiptFinishGoodBatches.RemoveAt(index);
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
        _receiptFinishGoodBatches.RemoveAt(Convert.ToInt32(dictionary["Index"]));
        _isBackFromBatch = false;
        FluentToast fluentToast = (FluentToast)dictionary["FluentToast"];
        fluentToast.Close();
        OnAddItemLineBack();
        return Task.CompletedTask;
    }

    private Task OnDeleteSerial(Dictionary<string, object> dictionary)
    {
        _receiptFinishGoodSerials.RemoveAt(Convert.ToInt32(dictionary["Index"]));
        _isBackFromBatch = false;
        FluentToast fluentToast = (FluentToast)dictionary["FluentToast"];
        fluentToast.Close();
        OnAddItemLineBack();
        return Task.CompletedTask;
    }

    private async Task OnConfirmLine()
    {
        DataResult.Batches = _receiptFinishGoodBatches;
        DataResult.Serials = _receiptFinishGoodSerials;
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
            { "qty", DataResult.Qty }
        };
        return await GetGenerateBatchSerial(data);
    }
}