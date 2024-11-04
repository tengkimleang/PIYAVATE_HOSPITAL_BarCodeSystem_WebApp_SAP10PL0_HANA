using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.VisualBasic;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.ReturnComponentProduction;
using Piyavate_Hospital.Shared.Services;
using Piyavate_Hospital.Shared.Views.GoodReceptPo;
using Piyavate_Hospital.Shared.Views.Shared.Component;

namespace Piyavate_Hospital.Shared.Views.ReturnComponent;

public partial class ReturnComponentDefault
{
    [Parameter] public string Token { get; set; } = string.Empty;
    [Inject] public IValidator<ReturnComponentProductionHeader>? Validator { get; init; }
    [Inject] public IValidator<ReturnComponentProductionLine>? ValidatorLine { get; init; }

    private string _stringDisplay = "Return From Component";
    private string _saveWord = "Save";
    private string? _dataGrid = "width: 1600px;height:405px";
    private bool _isView;

    private ObservableCollection<GetProductionOrderLines> _tmpGetProductionOrderLinesList =
        new ObservableCollection<GetProductionOrderLines>();

    protected override void OnInitialized()
    {
        ViewModel.Token = Token;
        ViewModel.LoadedCommand.ExecuteAsync(null).ConfigureAwait(false);
    }

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

    async Task<ObservableCollection<GetBatchOrSerial>> GetSerialBatch(Dictionary<string, string> dictionary)
    {
        await ViewModel.GetBatchOrSerialByItemCodeCommand.ExecuteAsync(dictionary);
        return ViewModel.GetBatchOrSerialsByItemCode;
    }

    async Task OpenDialogAsync(ReturnComponentProductionLine issueProductionLine)
    {
        IEnumerable<GetProductionOrderLines> listGetProductionOrderLines = ViewModel.GetProductionOrderLines
            .GroupBy(item => new
            {
                item.ItemCode,
                item.ItemName,
                item.Uom,
                item.WarehouseCode,
                item.ItemType
            })
            .Select(group => new GetProductionOrderLines
            {
                ItemCode = group.Key.ItemCode,
                ItemName = group.Key.ItemName,
                Qty = (group.Sum(x => Convert.ToDouble(x.Qty))).ToString(CultureInfo.InvariantCulture),
                Uom = group.First().Uom,
                WarehouseCode = group.First().WarehouseCode,
                ItemType = group.First().ItemType,
                DocEntry = group.First().DocEntry,
                PlanQty = group.Sum(x => Convert.ToDouble(x.PlanQty)).ToString(CultureInfo.InvariantCulture),
            }).ToImmutableList();

        var dictionary = new Dictionary<string, object>
        {
            { "item", listGetProductionOrderLines },
            { "line", issueProductionLine },
            { "warehouse", ViewModel.Warehouses },
            { "docNumOrderSelected", SelectedProductionOrder },
            {
                "getSerialBatch",
                new Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>>(GetSerialBatch)
            }
        };
        var dialog = await DialogService!.ShowDialogAsync<DialogAddLineReturnComponent>(dictionary
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
            if (data["data"] is ReturnComponentProductionLine issueProductionLineDialog)
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
            _stringDisplay = "Return From Component";
            _saveWord = "Save";
            _dataGrid = "width: 1600px;height:405px";
        }
    }

    private void DeleteLine(int index)
    {
        ViewModel.ReceiptFromProductionOrderForm.Lines.RemoveAt(index);
    }

    async Task OnSaveTransaction(string type = "")
    {
        _tmpGetProductionOrderLinesList = ViewModel.GetProductionOrderLines;
        var productionOrder = ViewModel.IssueProductionLine.ToList();
        ViewModel.ReceiptFromProductionOrderForm.Lines = new();
        var strMp = JsonSerializer.Serialize(ViewModel.IssueProductionLine.AsQueryable());
        var strGetProductionOrderLines = JsonSerializer.Serialize(ViewModel.GetProductionOrderLines.AsQueryable());
        await ErrorHandlingHelper.ExecuteWithHandlingAsync(async () =>
        {
            foreach (var line in productionOrder.ToList())
            {
                if (line.ManageItem == "N")
                {
                    ProcessItemNones(line);
                }
                else if (line.ManageItem == "B")
                {
                    ProcessItemBatch(line);
                }
                else if (line.ManageItem == "S")
                {
                    ProcessItemSerial(line);
                }
            }

            var result = await Validator!.ValidateAsync(ViewModel.ReceiptFromProductionOrderForm).ConfigureAwait(false);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ToastService!.ShowError(error.ErrorMessage);
                }

                ViewModel.IssueProductionLine =
                    JsonSerializer.Deserialize<ObservableCollection<ReturnComponentProductionLine>>(strMp) ?? new();
                ViewModel.GetProductionOrderLines =
                    JsonSerializer.Deserialize<ObservableCollection<GetProductionOrderLines>>(
                        strGetProductionOrderLines) ??
                    new();
                return;
            }

            _visible = true;
            await ViewModel.SubmitCommand.ExecuteAsync(null).ConfigureAwait(false);

            if (ViewModel.PostResponses.ErrorCode == "")
            {
                SelectedProductionOrder = new List<GetProductionOrder>();
                ViewModel.ReceiptFromProductionOrderForm = new ReturnComponentProductionHeader();
                ToastService.ShowSuccess("Success");
                if (type == "print") await OnSeleted(ViewModel.PostResponses.DocEntry);
                _visible = false;
                ViewModel.IssueProductionLine = new();
                ViewModel.GetProductionOrderLines = new();
                ViewModel.ReceiptFromProductionOrderForm = new();
            }
            else
                ToastService.ShowError(ViewModel.PostResponses.ErrorMsg);
        }, ViewModel.PostResponses, ToastService).ConfigureAwait(false);
        if (ViewModel.PostResponses.ErrorCode == "")
        {
            _visible = false;
            return;
        }

        Console.WriteLine(JsonSerializer.Serialize(ViewModel.ReceiptFromProductionOrderForm));
        ViewModel.IssueProductionLine =
            JsonSerializer.Deserialize<ObservableCollection<ReturnComponentProductionLine>>(strMp) ?? new();
        ViewModel.GetProductionOrderLines =
            JsonSerializer.Deserialize<ObservableCollection<GetProductionOrderLines>>(strGetProductionOrderLines) ??
            new();
        _visible = false;
    }

    async Task OnSeleted(string e)
    {
       await ViewModel.IssueForProductionDetailByDocNumCommand.ExecuteAsync(e).ConfigureAwait(false);
        _isView = true;
        StateHasChanged();
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

    #region OnSaveTransaction Logic

    void ProcessItemNones(ReturnComponentProductionLine line)
    {
        if (line.ItemNones != null)
        {
            foreach (var lineManual in line.ItemNones)
            {
                var totalManualQty = ViewModel.GetProductionOrderLines.Where(x =>
                        x.ItemCode == line.ItemCode
                        && x.DocEntry == lineManual.OnSelectedProductionOrder.FirstOrDefault()?.DocEntry
                    )
                    .Sum(x => Convert.ToDouble(x.Qty));
                foreach (var addLineManual in ViewModel.GetProductionOrderLines.Where(x =>
                             x.ItemCode == line.ItemCode
                             && x.DocEntry == lineManual.OnSelectedProductionOrder.FirstOrDefault()?.DocEntry))
                {
                    ViewModel.ReceiptFromProductionOrderForm.Lines.Add(new ReturnComponentProductionLine
                    {
                        DocNum = addLineManual.DocEntry,
                        BaseLineNum = Convert.ToInt32(addLineManual.OrderLineNum),
                        ItemCode = line.ItemCode,
                        ItemName = line.ItemName,
                        Qty = (Convert.ToDouble(addLineManual.Qty) / totalManualQty) * lineManual.Qty,
                        QtyRequire = line.QtyRequire,
                        QtyPlan = line.QtyPlan,
                        QtyManual = lineManual.Qty,
                        QtyLost = (Convert.ToDouble(addLineManual.Qty) / totalManualQty) * lineManual.QtyLost,
                        Price = line.Price,
                        WhsCode = line.WhsCode,
                        UomName = "Manual None",
                        ManageItem = "N"
                    });
                }
            }

            var total = ViewModel.GetProductionOrderLines.Where(x =>
                    x.ItemCode == line.ItemCode
                    && !ViewModel.ReceiptFromProductionOrderForm.Lines.Where(z => z.Qty > 0)
                        .Select(returnComponentProductionLine => returnComponentProductionLine.DocNum)
                        .Contains(x.DocEntry)
                )
                .Sum(x => Convert.ToDouble(x.Qty));
            var tmp = new List<ReturnComponentProductionLine>();
            foreach (var lineAuto in ViewModel.GetProductionOrderLines.Where(x =>
                         x.ItemCode == line.ItemCode
                         && !ViewModel.ReceiptFromProductionOrderForm.Lines.Where(z => z.Qty > 0)
                             .Select(returnComponentProductionLine => returnComponentProductionLine.DocNum)
                             .Contains(x.DocEntry)
                     ))
            {
                tmp.Add(new ReturnComponentProductionLine
                {
                    DocNum = lineAuto.DocEntry,
                    LineNum = line.LineNum,
                    BaseLineNum = Convert.ToInt32(lineAuto.OrderLineNum),
                    ItemCode = line.ItemCode,
                    ItemName = line.ItemName,
                    Qty = (Convert.ToDouble(lineAuto.Qty) / total) * line.Qty,
                    QtyRequire = line.QtyRequire,
                    QtyPlan = line.QtyPlan,
                    QtyManual = 0,
                    QtyLost = (Convert.ToDouble(lineAuto.Qty) / total) * line.QtyLost,
                    Price = line.Price,
                    WhsCode = line.WhsCode,
                    UomName = "Auto None",
                });
            }

            ViewModel.ReceiptFromProductionOrderForm.Lines.AddRange(tmp);
        }
    }

    #region Comment Batch

    void ProcessItemBatch(ReturnComponentProductionLine line)
    {
        {
            var tmpManual = new List<ReturnComponentProductionLine>();

            foreach (var lineManual in line.Batches.Where(x => x.QtyManual > 0 || x.QtyLost > 0).ToList())
            {
                // var selectedProductionOrderDocEntry = lineManual.OnSelectedProductionOrder.FirstOrDefault()?.DocEntry; && x.DocEntry == selectedProductionOrderDocEntry
                var matchingProductionOrderLines = ViewModel.GetProductionOrderLines
                    .Where(x => x.ItemCode == line.ItemCode &&
                                x.DocEntry == lineManual.OnSelectedProductionOrder.FirstOrDefault()?.DocEntry)
                    .ToList(); // Perform the filtering once and reuse the result.
                var totalManualQty = matchingProductionOrderLines
                    .Sum(x => Convert.ToDouble(x.Qty));
                foreach (var addLineManual in matchingProductionOrderLines)
                {
                    var batch = new List<BatchReturnComponentProduction>();
                    batch.Add(new()
                    {
                        BatchCode = lineManual.BatchCode,
                        Qty = Math.Round(
                            (Convert.ToDouble(addLineManual.Qty) / totalManualQty) * lineManual.QtyManual,
                            6),
                        ExpDate = lineManual.ExpDate,
                        ManfectureDate = lineManual.ManfectureDate,
                        AdmissionDate = lineManual.AdmissionDate,
                        LotNo = lineManual.LotNo
                    });
                    var manualLine = new ReturnComponentProductionLine
                    {
                        DocNum = addLineManual.DocEntry,
                        BaseLineNum = Convert.ToInt32(addLineManual.OrderLineNum),
                        ItemCode = line.ItemCode,
                        ItemName = line.ItemName,
                        Qty = Math.Round(
                            (Convert.ToDouble(addLineManual.Qty) / totalManualQty) * lineManual.QtyManual,
                            6),
                        QtyRequire = line.QtyRequire,
                        QtyPlan = Convert.ToDouble(addLineManual.Qty),
                        QtyManual = Convert.ToDouble(addLineManual.Qty),
                        QtyLost = Math.Round(
                            (Convert.ToDouble(addLineManual.Qty) / totalManualQty) * lineManual.QtyLost, 6),
                        Price = line.Price,
                        WhsCode = line.WhsCode,
                        UomName = "Manual Batch",
                        ManageItem = "B",
                        Type = 2,
                        Batches = batch
                    };
                    tmpManual.Add(manualLine);
                    line.Batches.Remove(lineManual);
                    ViewModel.GetProductionOrderLines.Remove(addLineManual);
                }
            }

            foreach (var lineAuto in line.Batches.ToList())
            {
                // var selectedProductionOrderDocEntry = lineManual.OnSelectedProductionOrder.FirstOrDefault()?.DocEntry; && x.DocEntry == selectedProductionOrderDocEntry
                var matchingProductionOrderLines = ViewModel.GetProductionOrderLines
                    .ToList(); // Perform the filtering once and reuse the result.
                if (!matchingProductionOrderLines.Any()) continue; // Skip if no matching lines found.

                var totalAutoQty = matchingProductionOrderLines
                    .Sum(x => Convert.ToDouble(x.Qty));
                var totalAutoLostQty = totalAutoQty;
                totalAutoQty += tmpManual.Where(z => z is { Qty: 0, UomName: "Manual Batch" })
                    .Sum(x => x.QtyPlan);
                totalAutoLostQty += tmpManual.Where(z => z is { QtyLost: 0, UomName: "Manual Batch" })
                    .Sum(x => x.QtyManual);
                foreach (var addLineAuto in matchingProductionOrderLines)
                {
                    var batch = new List<BatchReturnComponentProduction>();
                    batch.Add(new()
                    {
                        BatchCode = lineAuto.BatchCode,
                        Qty = Math.Round(
                            (Convert.ToDouble(addLineAuto.Qty) / totalAutoQty) * lineAuto.Qty,
                            6),
                        ExpDate = lineAuto.ExpDate,
                        ManfectureDate = lineAuto.ManfectureDate,
                        AdmissionDate = lineAuto.AdmissionDate,
                        LotNo = lineAuto.LotNo
                    });
                    var autoLine = new ReturnComponentProductionLine
                    {
                        DocNum = addLineAuto.DocEntry,
                        BaseLineNum = Convert.ToInt32(addLineAuto.OrderLineNum),
                        ItemCode = line.ItemCode,
                        ItemName = line.ItemName,
                        Qty = Math.Round((Convert.ToDouble(addLineAuto.Qty) / totalAutoQty) * lineAuto.Qty,
                            6),
                        QtyRequire = line.QtyRequire,
                        QtyPlan = line.QtyPlan,
                        QtyManual = lineAuto.QtyManual,
                        QtyLost = Math.Round(
                            (Convert.ToDouble(addLineAuto.Qty) / totalAutoLostQty) * line.QtyLost, 6),
                        Price = line.Price,
                        WhsCode = line.WhsCode,
                        ManageItem = "B",
                        UomName = "Auto Batch",
                        Type = 2,
                        Batches = batch
                    };
                    tmpManual.Where(z => z is { Qty: 0, UomName: "Manual Batch" })
                        .ToList().ForEach(x =>
                        {
                            x.Qty = Math.Round((Convert.ToDouble(x.QtyPlan) / totalAutoQty) * lineAuto.Qty,
                                6);
                            x.Batches.ForEach(y =>
                                y.Qty = Math.Round((Convert.ToDouble(x.QtyPlan) / totalAutoQty) * lineAuto.Qty, 6));
                        });
                    tmpManual.Where(z => z is { QtyLost: 0, UomName: "Manual Batch" })
                        .ToList().ForEach(x =>
                        {
                            x.QtyLost = Math.Round(
                                (Convert.ToDouble(x.QtyManual) / totalAutoLostQty) * line.QtyLost,
                                6);
                        });
                    tmpManual.Add(autoLine);
                    line.Batches.Remove(lineAuto);
                    ViewModel.GetProductionOrderLines.Remove(addLineAuto);
                }
            }

            ViewModel.ReceiptFromProductionOrderForm.Lines
                .AddRange(tmpManual); // Add outside the loop to avoid repeated additions.
        }
    }

    #endregion

    void ProcessItemSerial(ReturnComponentProductionLine line)
    {
        foreach (var lineManual in line.Serials)
        {
            ViewModel.ReceiptFromProductionOrderForm.Lines.Add(new ReturnComponentProductionLine
            {
                DocNum = line.DocNum,
                BaseLineNum = line.BaseLineNum,
                ItemCode = line.ItemCode,
                ItemName = line.ItemName,
                Qty = lineManual.Qty,
                QtyRequire = line.QtyRequire,
                QtyPlan = line.QtyPlan,
                QtyManual = lineManual.Qty,
                Price = line.Price,
                WhsCode = line.WhsCode,
                Serials = line.Serials,
                ManageItem = line.ManageItem
            });
        }
    }

    #endregion
}