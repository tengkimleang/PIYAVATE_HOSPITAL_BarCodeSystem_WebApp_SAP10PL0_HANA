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
using Piyavate_Hospital.Shared.Models.IssueForProduction;
using Piyavate_Hospital.Shared.Services;
using Piyavate_Hospital.Shared.Views.Shared.Component;

namespace Piyavate_Hospital.Shared.Views.IssueForProduction.MobileAppScreen.Add;

public partial class AddIssueForProductionMobile
{
    [Parameter] public string Token { get; set; } = string.Empty;
    [Inject] public IValidator<IssueProductionHeader>? Validator { get; init; }
    Dictionary<string, object> _lineItemContent = new();

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
        ComponentAttribute.Path = "/issueforproduction";
        ComponentAttribute.IsBackButton = true;
        ViewModel.Token = Token;
        ViewModel.LoadingCommand.ExecuteAsync(null).ConfigureAwait(false);   
    }

    async Task<ObservableCollection<GetBatchOrSerial>> GetSerialBatch(Dictionary<string, string> dictionary)
    {
        await ViewModel.GetBatchOrSerialByItemCodeCommand.ExecuteAsync(dictionary);
        return ViewModel.GetBatchOrSerialsByItemCode;
    }

    private Task OnAddLineItem(IssueProductionLine issueProductionLine)
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
            }).ToImmutableList();
        Console.WriteLine(JsonSerializer.Serialize(listGetProductionOrderLines));
        _lineItemContent = new Dictionary<string, object>
        {
            { "item", listGetProductionOrderLines },
            { "line", issueProductionLine },
            { "warehouse", ViewModel.Warehouses },
            {
                "getSerialBatch",
                new Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>>(GetSerialBatch)
            }
        };
        if (issueProductionLine.LineNum != 0)
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

    Task OnSaveItem(IssueProductionLine inventoryCountingLine)
    {
        if (inventoryCountingLine.LineNum == 0)
        {
            inventoryCountingLine.LineNum =
                ViewModel.IssueProductionLine.MaxBy(x => x.LineNum)?.LineNum + 1 ?? 1;
            ViewModel.IssueProductionLine.Add(inventoryCountingLine);
        }
        else
        {
            var index = ViewModel.IssueProductionForm.Lines.FindIndex(i =>
                i.LineNum == inventoryCountingLine.LineNum);
            ViewModel.IssueProductionLine[index] = inventoryCountingLine;
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
        await ErrorHandlingHelper.ExecuteWithHandlingAsync(async () =>
        {
            var issueProductionLines = ViewModel.IssueProductionLine.ToList();
            var serializedIssueProductionLine = JsonSerializer.Serialize(ViewModel.IssueProductionLine.AsQueryable());
            var serializedProductionOrderLines =
                JsonSerializer.Serialize(ViewModel.GetProductionOrderLines.AsQueryable());

            ViewModel.IssueProductionForm.Lines = new();

            foreach (var line in issueProductionLines)
            {
                var totalQty = ViewModel.GetProductionOrderLines?
                    .Where(x => x.ItemCode == line.ItemCode)
                    .Sum(x => Convert.ToDouble(x.Qty)) ?? 0;

                var productionOrderLines = ViewModel.GetProductionOrderLines!
                    .Where(x => x.ItemCode == line.ItemCode)
                    .ToList();

                foreach (var vmIssueProductionLine in productionOrderLines)
                {
                    var actualQty = Math.Round((Convert.ToDouble(vmIssueProductionLine.Qty) / totalQty) * line.Qty, 6);

                    if (line.ManageItem == "S")
                    {
                        ProcessSerials(line, vmIssueProductionLine, actualQty);
                    }
                    else if (line.ManageItem == "B")
                    {
                        ProcessBatches(line, vmIssueProductionLine, actualQty, issueProductionLines);
                    }
                    else
                    {
                        AddIssueProductionLine(vmIssueProductionLine, line, actualQty);
                    }
                }
            }

            RestoreViewModelState(serializedIssueProductionLine, serializedProductionOrderLines);

            Console.WriteLine(JsonSerializer.Serialize(ViewModel.IssueProductionForm));
            var result = await Validator!.ValidateAsync(ViewModel.IssueProductionForm).ConfigureAwait(false);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ToastService!.ShowError(error.ErrorMessage);
                }

                return;
            }

            await SubmitTransaction();
        }, ViewModel.PostResponses, ToastService).ConfigureAwait(false);
        StateHasChanged();
    }
    private async Task SubmitTransaction()
    {
        try
        {
            Visible = true;
            StateHasChanged();
            
            await ViewModel.SubmitCommand.ExecuteAsync(null).ConfigureAwait(false);

            if (string.IsNullOrEmpty(ViewModel.PostResponses.ErrorCode))
            {
                SelectedProductionOrder = new List<GetProductionOrder>();
                ViewModel.IssueProductionForm = new();
                ViewModel.IssueProductionLine = new();
                ToastService.ShowSuccess("Success");
            }
            else
            {
                ToastService.ShowError(ViewModel.PostResponses.ErrorMsg);
            }

            Visible = false;
            StateHasChanged();
        }
        catch (ApiException ex)
        {
            // var content = await ex.GetContentAsAsync<Dictionary<string, string>>();
            ToastService!.ShowError(ex.ReasonPhrase ?? "");
            Visible = false;
            StateHasChanged();
        }
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
    
    private void ProcessSerials(IssueProductionLine line, GetProductionOrderLines vmIssueProductionLine,
        double actualQty)
    {
        var serials = new List<SerialIssueProduction>();

        for (var i = 0; i < actualQty; i++)
        {
            if (line.Serials?.Count > 0)
            {
                serials.Add(line.Serials.First());
                line.Serials.RemoveAt(0);
            }
        }

        ViewModel.IssueProductionForm.Lines.Add(new IssueProductionLine
        {
            ItemCode = vmIssueProductionLine.ItemCode,
            ItemName = vmIssueProductionLine.ItemName,
            Qty = actualQty,
            UomName = vmIssueProductionLine.Uom,
            WhsCode = line.WhsCode,
            ManageItem = vmIssueProductionLine.ItemType,
            BaseLineNum = Convert.ToInt32(vmIssueProductionLine.OrderLineNum),
            DocNum = vmIssueProductionLine.DocEntry,
            Serials = serials,
        });
    }

    private void ProcessBatches(IssueProductionLine line, GetProductionOrderLines vmIssueProductionLine,
        double actualQty, List<IssueProductionLine> issueProductionLines)
    {
        var batches = new List<BatchIssueProduction>();
        var remainingQty = actualQty;

        foreach (var batch in line.Batches!.ToList())
        {
            if (remainingQty == 0) break;

            var index = line.Batches!.IndexOf(batch);
            var indexHeader = issueProductionLines.IndexOf(line);

            batches.Add(new BatchIssueProduction
            {
                AdmissionDate = batch.AdmissionDate,
                BatchCode = batch.BatchCode,
                ExpDate = batch.ExpDate,
                Qty = Math.Min(batch.Qty, remainingQty)
            });

            if (batch.Qty > remainingQty)
            {
                issueProductionLines[indexHeader].Batches![index].Qty = Math.Round(batch.Qty - remainingQty, 6);
                remainingQty = 0;
            }
            else
            {
                remainingQty -= batch.Qty;
                issueProductionLines[indexHeader].Batches![index].Qty = 0;
            }

            if (batch.Qty == 0)
            {
                line.Batches!.Remove(batch);
            }
        }

        if (batches.Count > 0)
        {
            ViewModel.IssueProductionForm.Lines.Add(new IssueProductionLine
            {
                ItemCode = vmIssueProductionLine.ItemCode,
                ItemName = vmIssueProductionLine.ItemName,
                Qty = actualQty,
                UomName = vmIssueProductionLine.Uom,
                WhsCode = line.WhsCode,
                ManageItem = vmIssueProductionLine.ItemType,
                BaseLineNum = Convert.ToInt32(vmIssueProductionLine.OrderLineNum),
                DocNum = vmIssueProductionLine.DocEntry,
                Batches = batches,
            });
        }
    }
    
    private void AddIssueProductionLine(GetProductionOrderLines vmIssueProductionLine, IssueProductionLine line,
        double actualQty)
    {
        ViewModel.IssueProductionForm.Lines.Add(new IssueProductionLine
        {
            ItemCode = vmIssueProductionLine.ItemCode,
            ItemName = vmIssueProductionLine.ItemName,
            Qty = actualQty,
            UomName = vmIssueProductionLine.Uom,
            WhsCode = line.WhsCode,
            ManageItem = vmIssueProductionLine.ItemType,
            BaseLineNum = Convert.ToInt32(vmIssueProductionLine.OrderLineNum),
            DocNum = vmIssueProductionLine.DocEntry
        });
    }

    private void RestoreViewModelState(string serializedIssueProductionLine, string serializedProductionOrderLines)
    {
        ViewModel.IssueProductionLine =
            JsonSerializer.Deserialize<ObservableCollection<IssueProductionLine>>(serializedIssueProductionLine) ??
            new();
        ViewModel.GetProductionOrderLines =
            JsonSerializer.Deserialize<ObservableCollection<GetProductionOrderLines>>(serializedProductionOrderLines) ??
            new();
    }
    
}