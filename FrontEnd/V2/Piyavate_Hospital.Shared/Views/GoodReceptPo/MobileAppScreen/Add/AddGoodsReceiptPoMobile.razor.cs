using System.Collections.ObjectModel;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.GoodReceiptPo;
using Piyavate_Hospital.Shared.Services;
using Piyavate_Hospital.Shared.Views.Shared.Component;

namespace Piyavate_Hospital.Shared.Views.GoodReceptPo.MobileAppScreen.Add;

public partial class AddGoodsReceiptPoMobile
{
    [Parameter] public int DocEntry { get; set; }
    [Parameter] public string Token { get; set; } = string.Empty;
    [Inject] public IValidator<GoodReceiptPoHeader>? Validator { get; init; }
    IEnumerable<Vendors> _selectedVendor = Array.Empty<Vendors>();
    Dictionary<string, object> _lineItemContent = new();
    bool _isItemLineClickAdd;
    private bool Visible { get; set; }

    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("goodreceptpoform");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        ComponentAttribute.Title = "List Search";
        ComponentAttribute.Path = "/goodreceptpoform";
        ComponentAttribute.IsBackButton = true;
        ViewModel.Token = Token;
        await ViewModel.LoadingCommand.ExecuteAsync(null).ConfigureAwait(false);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            ViewModel.GoodReceiptPoForm = new GoodReceiptPoHeader();
            if (DocEntry != 0)
            {
                ViewModel.Token = Token;
                await ViewModel.GetPurchaseOrderLineByDocNumCommand.ExecuteAsync(DocEntry.ToString())
                    .ConfigureAwait(false);
                ViewModel.GoodReceiptPoForm = new()
                {
                    VendorCode = ViewModel.GoodReceiptPoHeaderDetailByDocNums.FirstOrDefault()?.Vendor ?? "",
                    DocDate = DateTime.Now,
                    TaxDate = DateTime.Now,
                    Remarks = ViewModel.GoodReceiptPoHeaderDetailByDocNums.FirstOrDefault()?.RefInv ?? "",
                    Lines = ViewModel.GetPurchaseOrderLineByDocNums.Select((x,index) => new GoodReceiptPoLine()
                    {
                        ItemCode = x.ItemCode,
                        ItemName = x.ItemName,
                        LineNum = index+1,
                        ManageItem = x.ManageItem,
                        Qty = Convert.ToDouble(x.Qty),
                        Price = Convert.ToDouble(x.Price),
                        VatCode = x.VatCode,
                        WarehouseCode = x.WarehouseCode,
                        BaseLine = Convert.ToInt32(x.BaseLineNumber),
                        BaseEntry = Convert.ToInt32(x.DocEntry),
                    }).ToList()
                };
                _selectedVendor =
                    ViewModel.Vendors.Where(x => x.VendorCode == ViewModel.GoodReceiptPoHeaderDetailByDocNums.FirstOrDefault()?.Vendor)
                        .ToList();
                StateHasChanged();
            }
        }
    }

    private void OnSearch(OptionsSearchEventArgs<Vendors> e)
    {
        e.Items = ViewModel.Vendors.Where(i => i.VendorCode.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                                               i.VendorName.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.VendorCode);
    }

    // async Task<ObservableCollection<GetBatchOrSerial>> GetSerialBatch(Dictionary<string, string> dictionary)
    // {
    //     await ViewModel.GetBatchOrSerialByItemCodeCommand.ExecuteAsync(dictionary);
    //     return ViewModel.GetBatchOrSerialsByItemCode;
    // }

    private Task OnAddLineItem(GoodReceiptPoLine goodReceiptPoLine)
    {
        _lineItemContent = new Dictionary<string, object>
        {
            { "item", ViewModel.Items },
            { "taxPurchase", ViewModel.TaxPurchases },
            { "warehouse", ViewModel.Warehouses },
            { "line", goodReceiptPoLine },
        };
        Console.WriteLine(JsonSerializer.Serialize(goodReceiptPoLine));
        if (goodReceiptPoLine.LineNum != 0)
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
        Console.WriteLine(Convert.ToInt32(dictionary["Index"]));
        ViewModel.GoodReceiptPoForm.Lines?.RemoveAt(ViewModel.GoodReceiptPoForm.Lines.FindIndex(x =>
            x.LineNum == Convert.ToInt32(dictionary["Index"])));
        var fluentToast = (FluentToast)dictionary["FluentToast"];
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

    Task OnSaveItem(GoodReceiptPoLine goodReceiptPoLine)
    {
        if (goodReceiptPoLine.LineNum == 0)
        {
            goodReceiptPoLine.LineNum = ViewModel.GoodReceiptPoForm.Lines?.MaxBy(x => x.LineNum)?.LineNum + 1 ?? 1;
            Console.WriteLine(JsonSerializer.Serialize(goodReceiptPoLine));
            ViewModel.GoodReceiptPoForm.Lines ??= new();
            ViewModel.GoodReceiptPoForm.Lines?.Add(goodReceiptPoLine);
            Console.WriteLine(JsonSerializer.Serialize(ViewModel.GoodReceiptPoForm));
        }
        else
        {
            var index = ViewModel.GoodReceiptPoForm.Lines!.FindIndex(i => i.LineNum == goodReceiptPoLine.LineNum);
            ViewModel.GoodReceiptPoForm.Lines[index] = goodReceiptPoLine;
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
            ViewModel.GoodReceiptPoForm.VendorCode = _selectedVendor.FirstOrDefault()?.VendorCode ?? "";
            ViewModel.GoodReceiptPoForm.DocDate = DateTime.Now;
            var result = await Validator!.ValidateAsync(ViewModel.GoodReceiptPoForm).ConfigureAwait(false);
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
                _selectedVendor = new List<Vendors>();
                ViewModel.GoodReceiptPoForm = new GoodReceiptPoHeader();
                ToastService.ShowSuccess("Success");
                // if (type == "print") await OnSeleted(ViewModel.PostResponses.DocEntry.ToString());
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