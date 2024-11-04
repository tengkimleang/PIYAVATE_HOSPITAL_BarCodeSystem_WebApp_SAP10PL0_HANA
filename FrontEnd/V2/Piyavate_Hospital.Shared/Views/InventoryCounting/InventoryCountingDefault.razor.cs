using System.Collections.ObjectModel;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Refit;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.InventoryCounting;
using Piyavate_Hospital.Shared.Models.IssueForProduction;
using Piyavate_Hospital.Shared.Services;
using Piyavate_Hospital.Shared.Views.GoodReceptPo;

namespace Piyavate_Hospital.Shared.Views.InventoryCounting;

public partial class InventoryCountingDefault
{
    [Parameter] public string Token { get; set; } = string.Empty;
    [Inject] public IValidator<InventoryCountingHeader>? Validator { get; init; }
    [Inject] public IValidator<InventoryCountingLine>? ValidatorLine { get; init; }
    [Inject] public Blazored.LocalStorage.ISyncLocalStorageService? LocalStorage { get; init; }

    private string _stringDisplay = "Inventory Counting";
    private string _saveWord = "Save";
    string? _dataGrid = "width: 1600px;height:405px";
    bool _isView = false;
    protected void OnCloseOverlay() => _visible = true;

    private IEnumerable<GetInventoryCountingList> SelectedProductionOrder { get; set; } =
        new List<GetInventoryCountingList>();

    bool _visible;

    protected override void OnInitialized()
    {
        ViewModel.Token = Token;
        ViewModel.LoadingCommand.ExecuteAsync(null).ConfigureAwait(false);
    }

    async Task<ObservableCollection<GetBatchOrSerial>> GetSerialBatch(Dictionary<string, string> dictionary)
    {
        Console.WriteLine(JsonSerializer.Serialize(dictionary));
        await ViewModel.GetBatchOrSerialByItemCodeCommand.ExecuteAsync(dictionary);
        return ViewModel.GetBatchOrSerialsByItemCode;
    }

    async Task OpenDialogAsync(InventoryCountingLine issueProductionLine)
    {
        var dictionary = new Dictionary<string, object>
        {
            { "item", ViewModel.GetInventoryCountingLines },
            { "line", issueProductionLine },
            { "warehouse", ViewModel.Warehouses },
            {
                "getSerialBatch",
                new Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>>(GetSerialBatch)
            }
        };
        var dialog = await DialogService!.ShowDialogAsync<DialogAddLineInventoryCounting>(dictionary
            , new DialogParameters
            {
                Title = (issueProductionLine.ItemCode == "") ? "Add Line" : "Update Line",
                PreventDismissOnOverlayClick = true,
                PreventScroll = false,
                Width = "80%",
                Height = "80%"
            }).ConfigureAwait(false);

        var result = await dialog.Result.ConfigureAwait(false);
        if (!result.Cancelled && result.Data is Dictionary<string, object> data)
        {
            Console.WriteLine(JsonSerializer.Serialize(result.Data));
            if (ViewModel.InventoryCountingHeader?.Lines == null)
                ViewModel.InventoryCountingHeader!.Lines = new List<InventoryCountingLine>();
            if (data["data"] is InventoryCountingLine inventoryCountingLineDialog)
            {
                Console.WriteLine(JsonSerializer.Serialize(inventoryCountingLineDialog));
                if (inventoryCountingLineDialog.LineNum == 0)
                {
                    inventoryCountingLineDialog.LineNum =
                        ViewModel.InventoryCountingHeader?.Lines?.MaxBy(x => x.LineNum)?.LineNum + 1 ?? 1;
                    ViewModel.InventoryCountingHeader?.Lines?.Add(inventoryCountingLineDialog);
                }
                else
                {
                    var index = ViewModel.InventoryCountingHeader.Lines.ToList()
                        .FindIndex(i => i.LineNum == inventoryCountingLineDialog.LineNum);
                    ViewModel.InventoryCountingHeader.Lines[index] = inventoryCountingLineDialog;
                }
            }
        }
    }

    private void OnSearch(OptionsSearchEventArgs<GetInventoryCountingList> e)
    {
        e.Items = ViewModel.GetInventoryCountingLists.Where(i => i.Series.Contains(e.Text,
                                                                     StringComparison.OrdinalIgnoreCase) ||
                                                                 i.Series.Contains(e.Text
                                                                     , StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.Series);
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
            _stringDisplay = "Inventory Counting";
            _saveWord = "Save";
            _dataGrid = "width: 1600px;height:405px";
        }
    }

    private void DeleteLine(int index)
    {
        ViewModel.InventoryCountingHeader.Lines!.RemoveAt(index);
    }

    async Task OnSaveTransaction(string type = "")
    {
        await ErrorHandlingHelper.ExecuteWithHandlingAsync(async () =>
        {
            var result = await Validator!.ValidateAsync(ViewModel.InventoryCountingHeader).ConfigureAwait(false);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ToastService!.ShowError(error.ErrorMessage);
                }

                return;
            }

            _visible = true;
            ViewModel.InventoryCountingHeader.Lines.ForEach(x => x.LineNum = x.LineNum - 1);
            await SubmitTransaction(type);
        }, ViewModel.PostResponses, ToastService).ConfigureAwait(false);
        ViewModel.InventoryCountingHeader.Lines.ForEach(x => x.LineNum = x.LineNum + 1);
        _visible = false;
    }

    private void AddIssueProductionLine(GetProductionOrderLines vmIssueProductionLine, IssueProductionLine line,
        double actualQty)
    {
        ViewModel.InventoryCountingHeader.Lines.Add(new InventoryCountingLine
        {
            ItemCode = vmIssueProductionLine.ItemCode,
            // ItemName = vmIssueProductionLine.ItemName,
            Qty = actualQty,
            // UomName = vmIssueProductionLine.Uom,
            // WhsCode = line.WhsCode,
            ManageItem = vmIssueProductionLine.ItemType,
            // BaseLineNum = Convert.ToInt32(vmIssueProductionLine.OrderLineNum),
            // DocNum = vmIssueProductionLine.DocEntry
        });
    }

    private async Task SubmitTransaction(string type)
    {
        try
        {
            _visible = true;

            await ViewModel.SubmitCommand.ExecuteAsync(null).ConfigureAwait(false);

            if (string.IsNullOrEmpty(ViewModel.PostResponses.ErrorCode))
            {
                SelectedProductionOrder = new List<GetInventoryCountingList>();
                ViewModel.InventoryCountingHeader = new();
                ViewModel.InventoryCountingLines = new();
                ToastService.ShowSuccess("Success");

                if (type == "print")
                {
                    await OnSeleted(ViewModel.PostResponses.DocEntry.ToString());
                }
            }
            else
            {
                ToastService.ShowError(ViewModel.PostResponses.ErrorMsg);
            }

            _visible = false;
        }
        catch (ApiException ex)
        {
            var content = await ex.GetContentAsAsync<Dictionary<string, string>>();
            ToastService!.ShowError(ex.ReasonPhrase ?? "");
            _visible = false;
        }
    }

    async Task OnSeleted(string e)
    {
        // Console.WriteLine(e);
        await ViewModel.IssueForProductionDeatialByDocNumCommand.ExecuteAsync(e).ConfigureAwait(false);
        _isView = true;
        StateHasChanged();
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

    async Task<ObservableCollection<GetListData>> GetListData(int p)
    {
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
        await ViewModel.TotalCountInventoryCountingCommand.ExecuteAsync(null).ConfigureAwait(false);
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

    private async Task UpdateItemDetails(string? newValue)
    {
        if (!SelectedProductionOrder.Any())
        {
            ViewModel.InventoryCountingHeader.DocEntry = 0;
            ViewModel.InventoryCountingHeader.Series = "";
            ViewModel.InventoryCountingHeader.CreateDate = DateTime.Now;
            ViewModel.InventoryCountingHeader.CreateTime = "";
            ViewModel.InventoryCountingHeader.OtherRemark = "";
            ViewModel.InventoryCountingHeader.Ref2 = "";
            ViewModel.InventoryCountingHeader.InventoryCountingType = "";
            return;
        }

        ViewModel.InventoryCountingHeader.DocEntry = Convert.ToInt32(SelectedProductionOrder.ToList()[0].DocEntry);
        ViewModel.InventoryCountingHeader.Series = SelectedProductionOrder.ToList()[0].Series;
        ViewModel.InventoryCountingHeader.CreateDate =
            Convert.ToDateTime(SelectedProductionOrder.ToList()[0].CreateDate);
        ViewModel.InventoryCountingHeader.CreateTime = SelectedProductionOrder.ToList()[0].CreateTime;
        ViewModel.InventoryCountingHeader.OtherRemark = SelectedProductionOrder.ToList()[0].OtherRemark;
        ViewModel.InventoryCountingHeader.Ref2 = SelectedProductionOrder.ToList()[0].Ref2;
        ViewModel.InventoryCountingHeader.InventoryCountingType =
            SelectedProductionOrder.ToList()[0].InventoryCountingType;
        await ViewModel.GetPurchaseOrderLineByDocEntryCommand.ExecuteAsync(SelectedProductionOrder.ToList()[0].DocEntry)
            .ConfigureAwait(false);
        StateHasChanged();
    }
}