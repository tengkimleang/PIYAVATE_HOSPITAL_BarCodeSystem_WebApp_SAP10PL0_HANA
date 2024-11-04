using System.Collections.ObjectModel;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.InventoryTransfer;
using Piyavate_Hospital.Shared.Services;
using Piyavate_Hospital.Shared.ViewModels;
using Piyavate_Hospital.Shared.Views.GoodReceptPo;

namespace Piyavate_Hospital.Shared.Views.InventoryTransfer;

public partial class InventoryTransferDefault
{
    [Parameter] public InventoryTransferViewModel ViewModel { get; set; } = default!;
    [Parameter] public string Token { get; set; } = string.Empty;

    [Inject] public IValidator<InventoryTransferHeader>? Validator { get; init; }
    // [Inject]
    // public IValidator<InventoryTransferLine>? ValidatorLine { get; init; }

    private string _stringDisplay = "Inventory Transfer";
    // private string _fromWord = "From";
    private string _saveWord = "Save";
    string? _dataGrid = "width: 1600px;height:405px";
    bool _isView = false;
    protected void OnCloseOverlay() => _visible = true;

    bool _visible = false;

    protected override void OnInitialized()
    {
        ViewModel.Token = Token;
        ViewModel.LoadingCommand.ExecuteAsync(null).ConfigureAwait(false);
    }

    async Task OpenDialogAsync(InventoryTransferLine deliveryOrderLine)
    {
        var dictionary = new Dictionary<string, object>
        {
            { "item", ViewModel.Items },
            { "warehouse", ViewModel.Warehouses },
            { "line", deliveryOrderLine },
            {
                "getSerialBatch",
                new Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>>(GetSerialBatch)
            }
        };

        var dialog = await DialogService!.ShowDialogAsync<DialogAddLineInventoryTrasfer>(dictionary,
            new DialogParameters
            {
                Title = string.IsNullOrEmpty(deliveryOrderLine.ItemCode) ? "Add Line" : "Update Line",
                PreventDismissOnOverlayClick = true,
                PreventScroll = false,
                Width = "80%",
                Height = "80%"
            }).ConfigureAwait(false);

        var result = await dialog.Result.ConfigureAwait(false);
        if (!result.Cancelled && result.Data is Dictionary<string, object> data)
        {
            if (ViewModel.InventoryTransferForm.Lines.Count == 0) ViewModel.InventoryTransferForm.Lines = new();
            if (data["data"] is InventoryTransferLine receiptPoLine)
            {
                if (receiptPoLine.LineNum == 0)
                {
                    receiptPoLine.LineNum =
                        ViewModel.InventoryTransferForm.Lines.MaxBy(x => x.LineNum)?.LineNum + 1 ?? 1;
                    ViewModel.InventoryTransferForm.Lines.Add(receiptPoLine);
                }
                else
                {
                    var index = ViewModel.InventoryTransferForm.Lines.FindIndex(i =>
                        i.LineNum == receiptPoLine.LineNum);
                    ViewModel.InventoryTransferForm.Lines[index] = receiptPoLine;
                }
            }
        }
    }

    void UpdateGridSize(GridItemSize size)
    {
        if (size == GridItemSize.Xs)
        {
            _stringDisplay = "";
            _dataGrid = "width: 1600px;height:205px";
            // _fromWord = "";
            _saveWord = "S-";
        }
        else
        {
            _stringDisplay = "Inventory Transfer";
            // _fromWord = "From";
            _saveWord = "Save";
            _dataGrid = "width: 1600px;height:405px";
        }
    }

    private void DeleteLine(int index)
    {
        ViewModel.InventoryTransferForm.Lines.RemoveAt(index);
    }

    async Task OnSaveTransaction(string type = "")
    {
        await ErrorHandlingHelper.ExecuteWithHandlingAsync(async () =>
        {
            ViewModel.InventoryTransferForm.DocDate = DateTime.Now;
            var result = await Validator!.ValidateAsync(ViewModel.InventoryTransferForm).ConfigureAwait(false);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ToastService!.ShowError(error.ErrorMessage);
                }

                return;
            }

            _visible = true;

            await ViewModel.SubmitCommand.ExecuteAsync(null).ConfigureAwait(false);

            if (ViewModel.PostResponses.ErrorCode == "")
            {
                ViewModel.InventoryTransferForm = new InventoryTransferHeader();
                ToastService.ShowSuccess("Success");
                if (type == "print") await OnSeleted(ViewModel.PostResponses.DocEntry);
            }
            else
                ToastService.ShowError(ViewModel.PostResponses.ErrorMsg);
        }, ViewModel.PostResponses, ToastService).ConfigureAwait(false);
        _visible = false;
    }

    Task OnSeleted(string e)
    {
        Console.WriteLine(e);
        ViewModel.GetGoodReceiptPoHeaderDeatialByDocNumCommand.ExecuteAsync(e).ConfigureAwait(false);
        _isView = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    Task OnDelete(string e)
    {
        Console.WriteLine(e);
        return Task.CompletedTask;
    }

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
        await ViewModel.TotalCountInventoryTransferCommand.ExecuteAsync(null).ConfigureAwait(false);
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
            Title = "List Inventory Transfer",
            PreventDismissOnOverlayClick = true,
            PreventScroll = false,
            Width = "80%",
            Height = "80%"
        }).ConfigureAwait(false);
    }

    async Task<ObservableCollection<GetBatchOrSerial>> GetSerialBatch(Dictionary<string, string> dictionary)
    {
        await ViewModel.GetBatchOrSerialByItemCodeCommand.ExecuteAsync(dictionary);
        return ViewModel.GetBatchOrSerialsByItemCode;
    }

    async Task ListCopyFromPurchaseOrder()
    {
        var dictionary = new Dictionary<string, object>
        {
            { "totalItemCount", ViewModel.TotalItemCountSalesOrder },
            { "getData", new Func<int, Task<ObservableCollection<GetListData>>>(GetListDataPurchaseOrder) },
            //{ "isDelete", true },
            //{"onDelete",new Func<string,Task>(OnDelete)},
            { "isSelete", true },
            { "onSelete", new Func<string, Task>(OnSeletedPurchaseOrder) },
        };
        await DialogService!.ShowDialogAsync<ListGoodReceiptPo>(dictionary, new DialogParameters
        {
            Title = "List Sales Order",
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
        ViewModel.InventoryTransferForm.DocDate = Convert.ToDateTime(objData?.DocDate);
        ViewModel.InventoryTransferForm.TaxDate = Convert.ToDateTime(objData?.TaxDate);
        await ViewModel.GetPurchaseOrderLineByDocNumCommand.ExecuteAsync(e).ConfigureAwait(false);
        ViewModel.InventoryTransferForm.Lines = new List<InventoryTransferLine>();
        var i = 1;
        foreach (var obj in ViewModel.GetPurchaseOrderLineByDocNums)
        {
            ViewModel.InventoryTransferForm.Lines?.Add(new InventoryTransferLine()
            {
                ItemCode = obj.ItemCode,
                ItemName = obj.ItemName,
                Qty = Convert.ToDouble(obj.Qty),
                ManageItem = obj.ManageItem,
                LineNum = i,
                BaseEntry = Convert.ToInt32(obj.DocEntry),
                BaseLine = Convert.ToInt32(obj.BaseLineNumber),
            });
            i++;
        }

        StateHasChanged();
    }
}