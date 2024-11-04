
using Piyavate_Hospital.Shared.Models.GoodReceiptPo;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;
namespace Piyavate_Hospital.Shared.Views.GoodReceptPo;

public partial class DialogAddLineGoodReceiptPo
{
    [Inject]
    public IValidator<GoodReceiptPoLine>? Validator { get; init; }

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    [Parameter]
    public Dictionary<string, object> Content { get; set; } = default!;

    private GoodReceiptPoLine DataResult { get; set; } = new();
    private List<BatchReceiptPo> _batchReceiptPOs = new();
    private List<SerialReceiptPo> _serialReceiptPo = new();
    private bool _isItemBatch;
    private bool _isItemSerial;
    private IEnumerable<Items> _selectedItem = Array.Empty<Items>();
    private IEnumerable<Items> Items => Content["item"] as IEnumerable<Items> ?? new List<Items>();
    private Func<Dictionary<string, object>, Task<string>> GetGenerateBatchSerial => Content["getGenerateBatchSerial"] as Func<Dictionary<string,object>, Task<string>> ?? default!;
    private IEnumerable<VatGroups>? VatGroups => Content["taxPurchase"] as IEnumerable<VatGroups>;
    private IEnumerable<Warehouses>? Warehouses => Content["warehouse"] as IEnumerable<Warehouses>;
    string? dataGrid = "width: 1600px;";

    protected override void OnInitialized()
    {
        if (Content.TryGetValue("line", out var value))
        {
            DataResult = value as GoodReceiptPoLine ?? new GoodReceiptPoLine();
            _batchReceiptPOs = DataResult.Batches ?? new List<BatchReceiptPo>();
            _serialReceiptPo = DataResult.Serials ?? new List<SerialReceiptPo>();
            _selectedItem = Items.Where(i => i.ItemCode == DataResult.ItemCode);
            UpdateItemDetails(DataResult.ItemCode);
        }
    }

    private void OnSearch(OptionsSearchEventArgs<Items> e)
    {
        e.Items = Items.Where(i => i.ItemCode.Contains(e.Text, StringComparison.OrdinalIgnoreCase) ||
                            i.ItemName.Contains(e.Text, StringComparison.OrdinalIgnoreCase))
                            .OrderBy(i => i.ItemCode);
    }

    private async Task SaveAsync()
    {
        DataResult.Batches = _batchReceiptPOs;
        DataResult.Serials = _serialReceiptPo;
        var result = await Validator!.ValidateAsync(DataResult).ConfigureAwait(false);
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                ToastService!.ShowError(error.ErrorMessage);
            }
            return;
        }
        await Dialog.CloseAsync(new Dictionary<string, object>
        {
            { "data", DataResult },
            { "isUpdate", Content.ContainsKey("line") }
        });
    }

    private void UpdateItemDetails(string? newValue)
    {
        var firstItem = _selectedItem.FirstOrDefault();
        DataResult.Price = double.Parse(firstItem?.PriceUnit ?? "0");
        DataResult.ItemCode = firstItem?.ItemCode ?? "";
        DataResult.ItemName = firstItem?.ItemName ?? "";
        DataResult.ManageItem = firstItem?.ItemType;
        _isItemBatch = firstItem?.ItemType == "B";
        _isItemSerial = firstItem?.ItemType == "S";
    }

    private void AddLineToBatchOrSerial()
    {
        if (_isItemBatch)
        {
            _batchReceiptPOs.Add(new BatchReceiptPo { BatchCode = "", });
        }
        else if (_isItemSerial && _serialReceiptPo.Count() < DataResult.Qty)
        {
            _serialReceiptPo.Add(new SerialReceiptPo { SerialCode = "", });
        }
    }

    private void DeleteLineFromBatchOrSerial(int index)
    {
        if (_isItemBatch)
        {
            _batchReceiptPOs.RemoveAt(index);
        }
        else if (_isItemSerial)
        {
            _serialReceiptPo.RemoveAt(index);
        }
    }

    private void UpdateGridSize(GridItemSize size)
    {
        dataGrid = size == GridItemSize.Xs ? "width: 1200px;height:205px" : "width: 100%;height:405px";
    }
    private async Task OnClickGennerateBatchSerial(int index)
    {
        var data = new Dictionary<string, object>
        {
            { "itemCode", DataResult.ItemCode },
            { "qty", DataResult.Qty ?? 0 }
        };
        if(_isItemBatch)
            _batchReceiptPOs[index].BatchCode=(await GetGenerateBatchSerial(data));
        else if(_isItemSerial)
            _serialReceiptPo[index].SerialCode=await GetGenerateBatchSerial(data);
    }
}
