using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.VisualBasic;
using Refit;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.ReceiptFinishGood;
using Piyavate_Hospital.Shared.Services;
using Piyavate_Hospital.Shared.Views.GoodReceptPo;
using Piyavate_Hospital.Shared.Views.Shared.Component;

namespace Piyavate_Hospital.Shared.Views.ReceiptFromProduction;

public partial class ReceiptFromProductionDefault
{
    [Parameter] public string Token { get; set; } = string.Empty;
    [Inject] public IValidator<ReceiptFinishGoodHeader>? Validator { get; init; }
    // [Inject] public IValidator<ReceiptFinishGoodLine>? ValidatorLine { get; init; }

    private string _stringDisplay = "Receipts Finished Good";
    private string _saveWord = "Save";
    private string? _dataGrid = "width: 1600px;height:405px";
    private bool _isView;

//ViewModel.IssueProductionLine
    protected void OnCloseOverlay() => _visible = true;
    private IEnumerable<GetProductionOrder> _getProductionOrder = new List<GetProductionOrder>();

    private IEnumerable<GetProductionOrder> SelectedProductionOrder
    {
        get => _getProductionOrder;
        set
        {
            if (value.Count() != 0)
            {
                string param = String.Empty;
                foreach (var obj in value)
                {
                    param = param + "''" + obj.DocEntry + "'',";
                }

                param = Strings.Left(param, Strings.Len(param) - 3);
                param += "''";
                ViewModel.GetPurchaseOrderLineByDocEntryCommand.ExecuteAsync(param).ConfigureAwait(false);
            }
            else
            {
                ViewModel.GetProductionOrderLines = new();
            }

            _getProductionOrder = value;
        }
    }

    bool _visible;
    protected override void OnInitialized()
    {
        ViewModel.Token = Token;
        ViewModel.LoadedCommand.ExecuteAsync(null).ConfigureAwait(false);
    }

    private async Task<string> OnGetGenerateBatchOrSerial(Dictionary<string, object> e)
    {
        await ViewModel.GetGennerateBatchSerialCommand.ExecuteAsync(e);
        return ViewModel.GetGenerateBatchSerial.FirstOrDefault()?.BatchOrSerial ?? "";
    }

    async Task OpenDialogAsync(ReceiptFinishGoodLine issueProductionLine)
    {
        var dictionary = new Dictionary<string, object>
        {
            { "item", ViewModel.GetProductionOrderLines },
            { "line", issueProductionLine },
            { "getGenerateBatchSerial", new Func<Dictionary<string, object>, Task<string>>(OnGetGenerateBatchOrSerial) }
        };
        var dialog = await DialogService!.ShowDialogAsync<DialogAddLineReceiptFromProductionOrder>(dictionary
            , new DialogParameters
            {
                Title = (issueProductionLine.ItemCode == "") ? "Add Line" : "Update Line",
                PreventDismissOnOverlayClick = true,
                PreventScroll = false,
                Width = "80%",
                Height = "80%"
            }).ConfigureAwait(false);

        var result = await dialog.Result.ConfigureAwait(false);
        if (result is { Cancelled: false, Data: Dictionary<string, object> data })
        {
            if (data["data"] is ReceiptFinishGoodLine issueProductionLineDialog)
            {
                if (issueProductionLineDialog.LineNum == 0)
                {
                    issueProductionLineDialog.LineNum =
                        ViewModel.IssueProductionLine.MaxBy(x => x.LineNum)?.LineNum + 1 ?? 1;
                    ViewModel.IssueProductionLine.Add(issueProductionLineDialog);
                }
                else
                {
                    var index = ViewModel.IssueProductionLine.ToList()
                        .FindIndex(i => i.LineNum == issueProductionLineDialog.LineNum);
                    ViewModel.IssueProductionLine[index] = issueProductionLineDialog;
                }
            }
        }
    }

    private void OnSearch(OptionsSearchEventArgs<GetProductionOrder> e)
    {
        e.Items = ViewModel.GetProductionOrder.Where(i => i.DocNum.Contains(e.Text,
                                                              StringComparison.OrdinalIgnoreCase) ||
                                                          i.DocNum.Contains(e.Text
                                                              , StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.DocNum);
    }

    void UpdateGridSize(GridItemSize size)
    {
        if (size == GridItemSize.Xs)
        {
            _stringDisplay = "";
            _dataGrid = "width: 1600px;height:205px";
            _saveWord = "S-";
        }
        else
        {
            _stringDisplay = "Receipts Finished Good";
            _saveWord = "Save";
            _dataGrid = "width: 1600px;height:405px";
        }
    }

    private void DeleteLine(int index)
    {
        ViewModel.IssueProductionLine.RemoveAt(index);
    }

    async Task OnSaveTransaction(string type = "")
    {
        await ErrorHandlingHelper.ExecuteWithHandlingAsync(async () =>
        {
            ViewModel.ReceiptFromProductionOrderForm.DocDate = DateTime.Now;
            ViewModel.ReceiptFromProductionOrderForm.Lines = ViewModel.IssueProductionLine.ToList();
            var result = await Validator!.ValidateAsync(ViewModel.ReceiptFromProductionOrderForm).ConfigureAwait(false);
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
            Console.WriteLine(JsonSerializer.Serialize(ViewModel.PostResponses));
            if (ViewModel.PostResponses.ErrorCode == "")
            {
                _getProductionOrder = new List<GetProductionOrder>();
                ViewModel.ReceiptFromProductionOrderForm = new ReceiptFinishGoodHeader();
                ViewModel.IssueProductionLine = new();
                ToastService.ShowSuccess("Success");
                if (type == "print") await OnSeleted(ViewModel.PostResponses.DocEntry);
            }
            else
                ToastService.ShowError(ViewModel.PostResponses.ErrorMsg);
        }, ViewModel.PostResponses, ToastService).ConfigureAwait(false);
        _visible = false;
    }

    async Task OnSeleted(string e)
    {
        await ViewModel.IssueForProductionDetailByDocNumCommand.ExecuteAsync(e).ConfigureAwait(false);
        _isView = true;
        StateHasChanged();
        // return Task.CompletedTask;
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
        await ViewModel.TotalCountReceiptFromProductionCommand.ExecuteAsync(null).ConfigureAwait(false);
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
            Title = "List Issue For Production",
            PreventDismissOnOverlayClick = true,
            PreventScroll = false,
            Width = "80%",
            Height = "80%"
        }).ConfigureAwait(false);
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
    async Task OnPrintLayout()
    {
        await ViewModel.GetLayoutPrintCommand.ExecuteAsync(null).ConfigureAwait(false);
        var dictionary = new Dictionary<string, object>
        {
            { "getLayout", ViewModel.GetLayouts },
            { "docEntry", ViewModel.GoodReceiptPoHeaderDetailByDocNums.FirstOrDefault()?.DocEntry ?? "" },
        };
        await DialogService!.ShowDialogAsync<PrintLayout>(dictionary, new DialogParameters
        {
            Title = "Print Layout",
            PreventDismissOnOverlayClick = true,
            PreventScroll = false,
            Width = "40%",
            Height = "45%"
        }).ConfigureAwait(false);
    }
}