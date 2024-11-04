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
using Piyavate_Hospital.Shared.Views.Shared.Component;

namespace Piyavate_Hospital.Shared.Views.ReturnComponent.MobileAppScreen.Add;

public partial class AddReturnComponentMobile
{
    [Parameter] public string Token { get; set; } = string.Empty;
    [Inject] public IValidator<ReturnComponentProductionHeader>? Validator { get; init; }
    Dictionary<string, object> _lineItemContent = new();

    private ObservableCollection<GetProductionOrderLines> _tmpGetProductionOrderLinesList =
        [];

    private IEnumerable<GetProductionOrder> SelectedProductionOrder { get; set; } =
        new List<GetProductionOrder>();

    bool _isItemLineClickAdd;
    private bool Visible { get; set; }

    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("issueforproduction");
        }
    }

    protected override void OnInitialized()
    {
        ComponentAttribute.Title = "List Search";
        ComponentAttribute.Path = "/ReturnFromComponent";
        ComponentAttribute.IsBackButton = true;
        ViewModel.Token = Token;
        ViewModel.LoadedCommand.ExecuteAsync(null).ConfigureAwait(false);
    }

    async Task<ObservableCollection<GetBatchOrSerial>> GetSerialBatch(Dictionary<string, string> dictionary)
    {
        await ViewModel.GetBatchOrSerialByItemCodeCommand.ExecuteAsync(dictionary);
        return ViewModel.GetBatchOrSerialsByItemCode;
    }

    private Task OnAddLineItem(ReturnComponentProductionLine returnComponentProductionLine)
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

        _lineItemContent = new Dictionary<string, object>
        {
            { "item", listGetProductionOrderLines },
            { "line", returnComponentProductionLine },
            { "warehouse", ViewModel.Warehouses },
            { "docNumOrderSelected", SelectedProductionOrder },
            {
                "getSerialBatch",
                new Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>>(GetSerialBatch)
            }
        };
        if (returnComponentProductionLine.LineNum != 0)
        {
            _lineItemContent.Add("OnDeleteLineItem", new Func<int, Task>(OnDeleteItem));
        }

        _isItemLineClickAdd = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task OnDeleteItem(int lineNum)
    {
        ToastService.ShowToast<ToastCustom, Dictionary<string, object>>(
            new ToastParameters<Dictionary<string, object>>()
            {
                Intent = ToastIntent.Custom,
                Title = "Delete Item",
                Timeout = 6000,
                Icon = (new Icons.Regular.Size20.Delete(), Color.Accent),
                Content = new Dictionary<string, object>
                {
                    {
                        "Body", "Are you sure to Delete?"
                    },
                    {
                        "Index", lineNum
                    },
                    {
                        "OnClickPrimaryButton", new Func<Dictionary<string, object>, Task>(OnDeleteItemByLineNum)
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

    Task OnDeleteItemByLineNum(Dictionary<string, object> dictionary)
    {
        ViewModel.GetProductionOrderLines.RemoveAt((int)dictionary["Index"]);
        FluentToast fluentToast = (FluentToast)dictionary["FluentToast"];
        fluentToast.Close();
        OnAddItemLineBack();
        return Task.CompletedTask;
    }

    Task OnAddItemLineBack()
    {
        _isItemLineClickAdd = false;
        StateHasChanged();
        return Task.CompletedTask;
    }

    Task OnSaveItem(ReturnComponentProductionLine returnComponentProductionLine)
    {
        if (returnComponentProductionLine.LineNum == 0)
        {
            returnComponentProductionLine.LineNum =
                ViewModel.IssueProductionLine.MaxBy(x => x.LineNum)?.LineNum + 1 ?? 1;
            ViewModel.IssueProductionLine.Add(returnComponentProductionLine);
        }
        else
        {
            var index = ViewModel.IssueProductionLine.ToList().FindIndex(i =>
                i.LineNum == returnComponentProductionLine.LineNum);
            ViewModel.IssueProductionLine[index] = returnComponentProductionLine;
        }

        OnAddItemLineBack();
        return Task.CompletedTask;
    }

    Task OnConfirmTransactionDialog()
    {
        ToastService.ShowToast<ToastCustom, Dictionary<string, object>>(
            new ToastParameters<Dictionary<string, object>>()
            {
                Intent = ToastIntent.Custom,
                Title = "Confirmation",
                Timeout = 6000,
                Icon = (new Icons.Regular.Size20.CubeAdd(), Color.Accent),
                Content = new Dictionary<string, object>
                {
                    {
                        "Body", "Are you sure to Confirm?"
                    },
                    {
                        "Index", 1
                    },
                    {
                        "OnClickPrimaryButton", new Func<Dictionary<string, object>, Task>(OnConfirmTransaction)
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
        return Task.CompletedTask;
    }

    async Task OnConfirmTransaction(Dictionary<string, object> dictionary)
    {
        FluentToast fluentToast = (FluentToast)dictionary["FluentToast"];
        fluentToast.Close();
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

            Visible = true;
            StateHasChanged();
            await ViewModel.SubmitCommand.ExecuteAsync(null).ConfigureAwait(false);

            if (ViewModel.PostResponses.ErrorCode == "")
            {
                SelectedProductionOrder = new List<GetProductionOrder>();
                ViewModel.ReceiptFromProductionOrderForm = new ReturnComponentProductionHeader();
                ToastService.ShowSuccess("Success");

                Visible = false;
                ViewModel.IssueProductionLine = new();
                ViewModel.GetProductionOrderLines = new();
                ViewModel.ReceiptFromProductionOrderForm = new();
                StateHasChanged();
            }
            else
                ToastService.ShowError(ViewModel.PostResponses.ErrorMsg);
        }, ViewModel.PostResponses, ToastService).ConfigureAwait(false);
        if (ViewModel.PostResponses.ErrorCode == "")
        {
            Visible = false;
            StateHasChanged();
            return;
        }

        Console.WriteLine(JsonSerializer.Serialize(ViewModel.ReceiptFromProductionOrderForm));
        ViewModel.IssueProductionLine =
            JsonSerializer.Deserialize<ObservableCollection<ReturnComponentProductionLine>>(strMp) ?? new();
        ViewModel.GetProductionOrderLines =
            JsonSerializer.Deserialize<ObservableCollection<GetProductionOrderLines>>(strGetProductionOrderLines) ??
            new();
        Visible = false;
        StateHasChanged();
    }

    private void OnSearch(OptionsSearchEventArgs<GetProductionOrder> e)
    {
        e.Items = ViewModel.GetProductionOrder.Where(i => i.DocNum.Contains(e.Text,
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.DocNum);
    }


    private async Task UpdateItemDetails(string? newValue)
    {
        if (SelectedProductionOrder.Count() != 0)
        {
            string param = String.Empty;
            foreach (var obj in SelectedProductionOrder)
            {
                param = param + "''" + obj.DocEntry + "'',";
            }

            param = Strings.Left(param, Strings.Len(param) - 3);
            param += "''";
            await ViewModel.GetPurchaseOrderLineByDocEntryCommand.ExecuteAsync(param).ConfigureAwait(false);
        }
        else
        {
            ViewModel.GetProductionOrderLines = new();
        }

        StateHasChanged();
    }

    protected void OnCloseOverlay() => Visible = true;

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
        if (line.Batches != null)
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
                            x.Batches?.ForEach(y =>
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
        if (line.Serials != null)
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
                });
            }
        }
    }

    #endregion
}