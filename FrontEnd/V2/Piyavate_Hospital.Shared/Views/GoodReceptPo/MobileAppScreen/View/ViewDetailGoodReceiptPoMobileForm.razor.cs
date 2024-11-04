using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Views.Shared.Component;

namespace Piyavate_Hospital.Shared.Views.GoodReceptPo.MobileAppScreen.View;

public partial class ViewDetailGoodReceiptPoMobileForm
{
    [Parameter]
    public ObservableCollection<GoodReceiptPoHeaderDeatialByDocNum> GoodReceiptPoHeaderDeatialByDocNum { get; set; } =
        new();
    [Parameter] public Func<Task> IsViewDetail { get; set; }=default!;
    [Parameter] public ObservableCollection<GoodReceiptPoLineByDocNum> GoodReceiptPoLineByDocNum { get; set; } = new();
    [Parameter] public ObservableCollection<GetBatchOrSerial> GetBatchOrSerials { get; set; } = new();
    [Parameter] public ObservableCollection<GetLayout> PrintLayouts { get; set; } = new();
    bool _isShowBatchSerial=false;
    private string _itemCode=string.Empty;
    private string _lineNum=string.Empty;
    
    private Task OnViewItemDetail()
    {
        _isShowBatchSerial = false;
        StateHasChanged();
        return Task.CompletedTask;
    }
    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("goodreceptpoform");
        }
    }
    void ShowItemDetail(string itemCode, string lineNum)
    {
        this._itemCode = itemCode;
        this._lineNum = lineNum;
        _isShowBatchSerial = true;
        StateHasChanged();
    }
}