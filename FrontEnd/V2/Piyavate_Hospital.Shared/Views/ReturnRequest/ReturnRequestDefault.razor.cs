
using System.Collections.ObjectModel;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Services;
using Piyavate_Hospital.Shared.ViewModels;
using Piyavate_Hospital.Shared.Views.GoodReceptPo;

namespace Piyavate_Hospital.Shared.Views.ReturnRequest;

public partial class ReturnRequestDefault
{
    [Parameter] public ReturnRequestViewModel ViewModel { get; set; } = default!;
    [Parameter] public string Token { get; set; } = string.Empty;
    private bool Visible { get; set; }
    protected void OnCloseOverlay() => Visible = true;

    [Inject] public IValidator<DeliveryOrderHeader>? Validator { get; init; }
    // [Inject] public IValidator<DeliveryOrderLine>? ValidatorLine { get; init; }

    private string _stringDisplay = "Return Request";
    private string _fromWord = "From";
    private string _saveWord = "Save";
    string? _dataGrid = "width: 1600px;height:405px";
    bool _isView;
    
    IEnumerable<Vendors> _selectedVendor = Array.Empty<Vendors>();
    protected override void OnInitialized()
    {
        ViewModel.Token = Token;
        ViewModel.LoadingCommand.ExecuteAsync(null).ConfigureAwait(false);
    }
    async Task OpenDialogAsync(DeliveryOrderLine deliveryOrderLine)
    {
        Console.WriteLine(JsonSerializer.Serialize(deliveryOrderLine));
        var dictionary = new Dictionary<string, object>
        {
            { "item", ViewModel.Items },
            { "taxPurchase", ViewModel.TaxSales },
            { "warehouse", ViewModel.Warehouses },
            { "line", deliveryOrderLine },
            {
                "getSerialBatch",
                new Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>>(GetSerialBatch)
            }   
        };

        var dialog = await DialogService!.ShowDialogAsync<DialogAddLineReturnRequest>(dictionary, new DialogParameters
        {
            Title = (string.IsNullOrEmpty(deliveryOrderLine.ItemCode)) ? "Add Line" : "Update Line",
            PreventDismissOnOverlayClick = true,
            PreventScroll = false,
            Width = "80%",
            Height = "80%"
        }).ConfigureAwait(false);

        var result = await dialog.Result.ConfigureAwait(false);
        if (result is { Cancelled: false, Data: Dictionary<string, object> data })
        {
            ViewModel.DeliveryOrderForm.Lines ??= new();
            if (data["data"] is DeliveryOrderLine receiptPoLine)
            {
                if (receiptPoLine.LineNum == 0)
                {
                    receiptPoLine.LineNum = ViewModel.DeliveryOrderForm.Lines?.MaxBy(x => x.LineNum)?.LineNum + 1 ?? 1;
                    ViewModel.DeliveryOrderForm.Lines?.Add(receiptPoLine);
                }
                else
                {
                    var index = ViewModel.DeliveryOrderForm.Lines.FindIndex(i => i.LineNum == receiptPoLine.LineNum);
                    ViewModel.DeliveryOrderForm.Lines[index] = receiptPoLine;
                }
            }
        }
    }

    private void OnSearch(OptionsSearchEventArgs<Vendors> e)
    {
        e.Items = ViewModel.Customers.Where(i => i.VendorCode.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                                                 i.VendorName.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.VendorCode);
    }

    void UpdateGridSize(GridItemSize size)
    {
        if (size == GridItemSize.Xs)
        {
            _stringDisplay = "";
            _dataGrid = "width: 1600px;height:205px";
            _fromWord = "";
            _saveWord = "S-";
        }
        else
        {
            _stringDisplay = "Return Request";
            _fromWord = "From";
            _saveWord = "Save";
            _dataGrid = "width: 1600px;height:405px";
        }
    }

    private void DeleteLine(int index)
    {
        ViewModel.DeliveryOrderForm.Lines!.RemoveAt(index);
    }

    async Task OnSaveTransaction(string type = "")
    {
        await ErrorHandlingHelper.ExecuteWithHandlingAsync(async () =>
        {
            ViewModel.DeliveryOrderForm.CustomerCode = _selectedVendor.FirstOrDefault()?.VendorCode ?? "";
            ViewModel.DeliveryOrderForm.DocDate = DateTime.Now;
            var result = await Validator!.ValidateAsync(ViewModel.DeliveryOrderForm).ConfigureAwait(false);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ToastService!.ShowError(error.ErrorMessage);
                }
                return;
            }
            Visible = true;
            await ViewModel.SubmitCommand.ExecuteAsync(null).ConfigureAwait(false);

            if (ViewModel.PostResponses.ErrorCode == "")
            {
                _selectedVendor = new List<Vendors>();
                ViewModel.DeliveryOrderForm = new DeliveryOrderHeader();
                ToastService.ShowSuccess("Success");
                if (type == "print") await OnSeleted(ViewModel.PostResponses.DocEntry);
            }
            else
                ToastService.ShowError(ViewModel.PostResponses.ErrorMsg);
        }, ViewModel.PostResponses, ToastService).ConfigureAwait(false);
        Visible = false;
    }

    Task OnSeleted(string e)
    {
        Console.WriteLine(e);
        ViewModel.GetGoodReceiptPoHeaderDeatialByDocNumCommand.ExecuteAsync(e).ConfigureAwait(false);
        _isView = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    // Task OnDelete(string e)
    // {
    //     Console.WriteLine(e);
    //     return Task.CompletedTask;
    // }

    Task OnView()
    {
        _isView = false;
        StateHasChanged();
        return Task.CompletedTask;
    }

    async Task OnGetBatchOrSerial()
    {
        var dictionary = new Dictionary<string, object>
        {
            { "getData", ViewModel.GetBatchOrSerials },
        };
        await DialogService!.ShowDialogAsync<ListBatchOrSerial>(dictionary, new DialogParameters
        {
            Title = "List Batch Or Serial",
            PreventDismissOnOverlayClick = true,
            PreventScroll = false,
            Width = "80%",
            Height = "80%"
        }).ConfigureAwait(false);
    }

    async Task<ObservableCollection<GetListData>> GetListData(int p)
    {
        //OnGetPurchaseOrder
        await ViewModel.GetGoodReceiptPoCommand.ExecuteAsync(p.ToString());
        return ViewModel.GetListData;
    }

    async Task<ObservableCollection<GetListData>> OnSearchGoodReceiptPo(Dictionary<string, object> e)
    {
        await ViewModel.GetGoodReceiptPoBySearchCommand.ExecuteAsync(e);
        return ViewModel.GetListData;
    }

    async Task OpenListDataAsyncAsync()
    {
        await ViewModel.TotalCountReturnCommand.ExecuteAsync(null).ConfigureAwait(false);
        var dictionary = new Dictionary<string, object>
        {
            { "totalItemCount", ViewModel.TotalItemCount },
            { "getData", new Func<int, Task<ObservableCollection<GetListData>>>(GetListData) },
            //{ "isDelete", true },
            { "isSelete", true },
            { "onSelete", new Func<string, Task>(OnSeleted) },
            {
                "onSearch",
                new Func<Dictionary<string, object>, Task<ObservableCollection<GetListData>>>(OnSearchGoodReceiptPo)
            },
            //{"onDelete",new Func<string,Task>(OnDelete)},
        };
        await DialogService!.ShowDialogAsync<ListGoodReceiptPo>(dictionary, new DialogParameters
        {
            Title = "List Return Request",
            PreventDismissOnOverlayClick = true,
            PreventScroll = false,
            Width = "80%",
            Height = "80%"
        }).ConfigureAwait(false);
    }

    async Task<ObservableCollection<GetBatchOrSerial>> GetSerialBatch(Dictionary<string, string> dictionary)
    {
        Console.WriteLine(JsonSerializer.Serialize(dictionary));
        await ViewModel.GetBatchOrSerialByItemCodeCommand.ExecuteAsync(dictionary);
        return ViewModel.GetBatchOrSerialsByItemCode;
    }

    async Task ListCopyFromPurchaseOrder()
    {
        await ViewModel.TotalCountDeliveryOrderReturnCommand.ExecuteAsync(null).ConfigureAwait(false);
        var dictionary = new Dictionary<string, object>
        {
            { "totalItemCount", ViewModel.TotalItemCountDeliveryOrder },
            { "getData", new Func<int, Task<ObservableCollection<GetListData>>>(GetListDataPurchaseOrder) },
            //{ "isDelete", true },
            //{"onDelete",new Func<string,Task>(OnDelete)},
            { "isSelete", true },
            { "onSelete", new Func<string, Task>(OnSeletedPurchaseOrder) },
        };
        await DialogService!.ShowDialogAsync<ListGoodReceiptPo>(dictionary, new DialogParameters
        {
            Title = "List Delivery Order",
            PreventDismissOnOverlayClick = true,
            PreventScroll = false,
            Width = "80%",
            Height = "80%"
        }).ConfigureAwait(false);
    }

    async Task<ObservableCollection<GetListData>> GetListDataPurchaseOrder(int p)
    {
        await ViewModel.GetPurchaseOrderCommand.ExecuteAsync(p.ToString());
        return ViewModel.GetListData;
    }

    async Task OnSeletedPurchaseOrder(string e)
    {
        Console.WriteLine(e);
        var objData = ViewModel.GetListData.FirstOrDefault(x => x.DocEntry.ToString() == e);
        ViewModel.DeliveryOrderForm.DocDate = Convert.ToDateTime(objData?.DocDate);
        ViewModel.DeliveryOrderForm.TaxDate = Convert.ToDateTime(objData?.TaxDate);
        _selectedVendor = ViewModel.Customers.Where(x => x.VendorCode == objData?.VendorCode);
        await ViewModel.GetPurchaseOrderLineByDocNumCommand.ExecuteAsync(e).ConfigureAwait(false);
        ViewModel.DeliveryOrderForm.Lines = new List<DeliveryOrderLine>();
        var i = 1;
        foreach (var obj in ViewModel.GetPurchaseOrderLineByDocNums)
        {
            var batch = new List<BatchDeliveryOrder>();
            var serial = new List<SerialDeliveryOrder>();
            foreach (var objBatch in obj.Batches)
            {
                batch.Add(new BatchDeliveryOrder
                {
                    AdmissionDate = objBatch.AdmissionDate,
                    BatchCode = objBatch.BatchCode,
                    ExpDate = objBatch.ExpDate,
                    LotNo = objBatch.LotNo,
                    ManfectureDate = objBatch.ManfectureDate,
                    Qty = objBatch.Qty,
                    QtyAvailable = objBatch.QtyAvailable,
                    OnSelectedBatchOrSerial = objBatch.OnSelectedBatchOrSerial,
                });
            }

            foreach (var objSerial in obj.Serials)
            {
                serial.Add(new SerialDeliveryOrder
                {
                    ExpDate = objSerial.ExpDate,
                    MfrDate = objSerial.MfrDate,
                    Qty = objSerial.Qty,
                    SerialCode = objSerial.SerialCode,
                    MfrNo = objSerial.MfrNo,
                    OnSelectedBatchOrSerial = objSerial.OnSelectedBatchOrSerial,
                });
            }

            ViewModel.DeliveryOrderForm.Lines?.Add(new DeliveryOrderLine()
            {
                ItemCode = obj.ItemCode,
                ItemName = obj.ItemName,
                Qty = Convert.ToDouble(obj.Qty),
                Price = Convert.ToDouble(obj.Price),
                VatCode = obj.VatCode,
                WarehouseCode = obj.WarehouseCode,
                ManageItem = obj.ManageItem,
                LineNum = i,
                BaseEntry = Convert.ToInt32(obj.DocEntry),
                BaseLine = Convert.ToInt32(obj.BaseLineNumber),
                Batches = batch,
                Serials = serial
            });
            i++;
        }

        StateHasChanged();
    }
}