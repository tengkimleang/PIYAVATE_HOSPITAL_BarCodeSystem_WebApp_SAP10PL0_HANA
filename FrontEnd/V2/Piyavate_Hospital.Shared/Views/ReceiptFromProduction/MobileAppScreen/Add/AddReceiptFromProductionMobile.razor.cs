using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.VisualBasic;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.ReceiptFinishGood;
using Piyavate_Hospital.Shared.Services;
using Piyavate_Hospital.Shared.Views.Shared.Component;

namespace Piyavate_Hospital.Shared.Views.ReceiptFromProduction.MobileAppScreen.Add;

public partial class AddReceiptFromProductionMobile
{
    [Parameter] public string Token { get; set; } = string.Empty;
    [Inject] public IValidator<ReceiptFinishGoodHeader>? Validator { get; init; }
    Dictionary<string, object> _lineItemContent = new();
    bool _isItemLineClickAdd;
    private bool Visible { get; set; }

    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("ReceiptsFinishedGoods");
        }
    }

    protected override void OnInitialized()
    {
        ComponentAttribute.Title = "List Search";
        ComponentAttribute.Path = "/ReceiptsFinishedGoods";
        ComponentAttribute.IsBackButton = true;
        ViewModel.Token = Token;
        ViewModel.LoadedCommand.ExecuteAsync(null).ConfigureAwait(false);
    }

    private IEnumerable<GetProductionOrder> SelectedProductionOrder { get; set; } = default!;
    private void OnSearch(OptionsSearchEventArgs<GetProductionOrder> e)
    {
        e.Items = ViewModel.GetProductionOrder.Where(i => i.DocNum.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.DocNum);
    }
    private void UpdateItemDetails(string? valueNew = "")
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
            ViewModel.GetPurchaseOrderLineByDocEntryCommand.ExecuteAsync(param).ConfigureAwait(false);
        }
        else
        {
            ViewModel.GetProductionOrderLines = new();
        }
    }

    private Task OnAddLineItem(ReceiptFinishGoodLine receiptFinishGoodLine)
    {
        _lineItemContent = new Dictionary<string, object>
        {
            { "item", ViewModel.GetProductionOrderLines },
            { "line", receiptFinishGoodLine },
            { "getGenerateBatchSerial", new Func<Dictionary<string, object>, Task<string>>(OnGetGenerateBatchOrSerial) }
        };
        Console.WriteLine(JsonSerializer.Serialize(receiptFinishGoodLine));
        if (receiptFinishGoodLine.LineNum != 0)
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
        ViewModel.ReceiptFromProductionOrderForm.Lines.RemoveAt(ViewModel.ReceiptFromProductionOrderForm.Lines.FindIndex(x =>
            x.LineNum == Convert.ToInt32(dictionary["Index"])));
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

    Task OnSaveItem(ReceiptFinishGoodLine receiptFinishGoodLine)
    {
        if (receiptFinishGoodLine.LineNum == 0)
        {
            receiptFinishGoodLine.LineNum =
                ViewModel.IssueProductionLine.MaxBy(x => x.LineNum)?.LineNum + 1 ?? 1;
            ViewModel.IssueProductionLine.Add(receiptFinishGoodLine);
        }
        else
        {
            var index = ViewModel.IssueProductionLine.ToList()
                .FindIndex(i => i.LineNum == receiptFinishGoodLine.LineNum);
            ViewModel.IssueProductionLine[index] = receiptFinishGoodLine;
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

            Visible = true;
            StateHasChanged();
            await ViewModel.SubmitCommand.ExecuteAsync(null).ConfigureAwait(false);
            
            if (ViewModel.PostResponses.ErrorCode == "")
            {
                SelectedProductionOrder = new List<GetProductionOrder>();
                ViewModel.ReceiptFromProductionOrderForm = new ReceiptFinishGoodHeader();
                ViewModel.IssueProductionLine= new();
                ToastService.ShowSuccess("Success");
            }
            else
                ToastService.ShowError(ViewModel.PostResponses.ErrorMsg);
        }, ViewModel.PostResponses, ToastService).ConfigureAwait(false);
        Visible = false;
        StateHasChanged();
    }

    private async Task<string> OnGetGenerateBatchOrSerial(Dictionary<string, object> e)
    {
        await ViewModel.GetGennerateBatchSerialCommand.ExecuteAsync(e);
        return ViewModel.GetGenerateBatchSerial.FirstOrDefault()?.BatchOrSerial ?? "";
    }

    protected void OnCloseOverlay() => Visible = true;
}