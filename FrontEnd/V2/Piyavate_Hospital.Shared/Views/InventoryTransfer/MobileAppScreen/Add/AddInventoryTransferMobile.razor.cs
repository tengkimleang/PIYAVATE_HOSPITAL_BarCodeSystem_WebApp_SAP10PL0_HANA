using System.Collections.ObjectModel;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.InventoryTransfer;
using Piyavate_Hospital.Shared.Services;
using Piyavate_Hospital.Shared.Views.Shared.Component;

namespace Piyavate_Hospital.Shared.Views.InventoryTransfer.MobileAppScreen.Add;

public partial class AddInventoryTransferMobile
{
    [Parameter] public string Token { get; set; } = string.Empty;
    [Inject] public IValidator<InventoryTransferHeader>? Validator { get; init; }
    private IEnumerable<Warehouses>? _selectedWarehousesFrom = Array.Empty<Warehouses>();
    private IEnumerable<Warehouses>? _selectedWarehousesTo = Array.Empty<Warehouses>();
    Dictionary<string, object> _lineItemContent = new();
    bool _isItemLineClickAdd;
    private bool Visible { get; set; }

    private void OnSearchWarehousesFrom(OptionsSearchEventArgs<Warehouses> e)
    {
        e.Items = ViewModel.Warehouses.Where(i => i.Name.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                                                  i.Code.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.Code);
    }

    private void OnSearchWarehousesTo(OptionsSearchEventArgs<Warehouses> e)
    {
        e.Items = ViewModel.WarehousesTo.Where(i => i.Name.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                                                    i.Code.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.Code);
    }

    private Task UpdateWarehousesFrom(string newValue)
    {
        Console.WriteLine(newValue);
        if (_selectedWarehousesFrom != null)
        {
            var firstItem = _selectedWarehousesFrom.FirstOrDefault();
            if (firstItem != null)
                ViewModel.InventoryTransferForm.FromWarehouse = firstItem.Code;
        }

        return Task.CompletedTask;
    }

    private Task UpdateWarehousesTo(string newValue)
    {
        Console.WriteLine(newValue);
        if (_selectedWarehousesTo != null)
        {
            var firstItem = _selectedWarehousesTo.FirstOrDefault();
            if (firstItem != null)
                ViewModel.InventoryTransferForm.ToWarehouse = firstItem.Code;
        }

        return Task.CompletedTask;
    }

    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("inventorytransfer");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        ComponentAttribute.Title = "List Search";
        ComponentAttribute.Path = "/inventorytransfer";
        ComponentAttribute.IsBackButton = true;
        await ViewModel.LoadingCommand.ExecuteAsync(null).ConfigureAwait(false);
    }

    async Task<ObservableCollection<GetBatchOrSerial>> GetSerialBatch(Dictionary<string, string> dictionary)
    {
        await ViewModel.GetBatchOrSerialByItemCodeCommand.ExecuteAsync(dictionary);
        return ViewModel.GetBatchOrSerialsByItemCode;
    }

    private Task OnAddLineItem(InventoryTransferLine inventoryTransferLine)
    {
        Console.WriteLine(JsonSerializer.Serialize(inventoryTransferLine));
        _lineItemContent = new Dictionary<string, object>
        {
            { "item", ViewModel.Items },
            { "line", inventoryTransferLine },
            {
                "getSerialBatch",
                new Func<Dictionary<string, string>, Task<ObservableCollection<GetBatchOrSerial>>>(GetSerialBatch)
            }
        };
        if (inventoryTransferLine.LineNum != 0)
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
        ViewModel.InventoryTransferForm.Lines.RemoveAt(
            ViewModel.InventoryTransferForm.Lines.FindIndex(x => x.LineNum == (int)dictionary["Index"]));
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

    Task OnSaveItem(InventoryTransferLine inventoryTransferLine)
    {
        if (inventoryTransferLine.LineNum == 0)
        {
            inventoryTransferLine.LineNum =
                ViewModel.InventoryTransferForm.Lines.MaxBy(x => x.LineNum)?.LineNum + 1 ?? 1;
            Console.WriteLine(JsonSerializer.Serialize(inventoryTransferLine));
            // ViewModel.InventoryTransferForm.Lines ??= [];
            ViewModel.InventoryTransferForm.Lines.Add(inventoryTransferLine);
            Console.WriteLine(JsonSerializer.Serialize(ViewModel.InventoryTransferForm));
        }
        else
        {
            var index = ViewModel.InventoryTransferForm.Lines.FindIndex(i =>
                i.LineNum == inventoryTransferLine.LineNum);
            ViewModel.InventoryTransferForm.Lines[index] = inventoryTransferLine;
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

            Visible = true;
            StateHasChanged();
            await ViewModel.SubmitCommand.ExecuteAsync(null).ConfigureAwait(false);

            if (ViewModel.PostResponses.ErrorCode == "")
            {
                _selectedWarehousesFrom = new List<Warehouses>();
                _selectedWarehousesTo = new List<Warehouses>();
                ViewModel.InventoryTransferForm = new InventoryTransferHeader();
                ToastService.ShowSuccess("Success");
                // if (type == "print") await OnSeleted(ViewModel.PostResponses.DocEntry.ToString());
            }
            else
                ToastService.ShowError(ViewModel.PostResponses.ErrorMsg);
        }, ViewModel.PostResponses, ToastService).ConfigureAwait(false);
        Visible = false;
        StateHasChanged();
    }

    protected void OnCloseOverlay() => Visible = true;
}