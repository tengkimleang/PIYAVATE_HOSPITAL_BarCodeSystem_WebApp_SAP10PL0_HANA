using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.Views.ReturnRequest.MobileAppScreen.List;

public partial class ListSearchDeliveryOrderReturnRequest
{
    [Parameter] public string Token { get; set; } = string.Empty;
    private int _refreshCount;
    private int _count;
    private string? _searchValue;
    private readonly ObservableCollection<GetListData> _scrollingData = new();
    public bool IsViewDetail;

    protected override async void OnInitialized()
    {
        StateHasChanged();
        ViewModel.Token= Token;
        ComponentAttribute.Title = "List Search DO";
        ComponentAttribute.Path = "/ReturnRequest";
        ComponentAttribute.IsBackButton = true;
        await OnRefreshAsync();
        IsViewDetail = false;
    }

    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("ReturnRequest");
        }
    }

    private async Task OnSearch()
    {
        if (!string.IsNullOrWhiteSpace(_searchValue))
        {
            _scrollingData.Clear();
            _count = 0;
            _refreshCount = 0;
            var dataSearch = new Dictionary<string, object>
                { { "docNum", _searchValue }, { "dateFrom", "" }, { "dateTo", "" } };
            await ViewModel.GetGoodReceiptPoBySearchCommand.ExecuteAsync(dataSearch).ConfigureAwait(false);
            foreach (var item in ViewModel.GetListData)
            {
                _scrollingData.Add(item);
            }

            StateHasChanged();
        }
        else
        {
            await OnRefreshAsync().ConfigureAwait(false);
        }
    }

    public async Task<bool> OnRefreshAsync()
    {
        await ViewModel.TotalCountDeliveryOrderReturnCommand.ExecuteAsync(null).ConfigureAwait(false);
        if (Convert.ToInt32(ViewModel.TotalItemCountDeliveryOrder.FirstOrDefault()?.AllItem ?? "0") <= _count)
        {
            return false;
        }

        await ViewModel.GetPurchaseOrderCommand.ExecuteAsync(_refreshCount.ToString()).ConfigureAwait(false);
        foreach (var item in ViewModel.GetListData)
        {
            _scrollingData.Add(item);
        }

        _refreshCount++;
        _count = +_scrollingData.Count;
        StateHasChanged();
        return true;
    }

    private Task OnClickCopy(string docEntry)
    {
        NavigationManager.NavigateTo($"/ReturnRequest/Mobile/Add/{docEntry}");
        return Task.CompletedTask;
    }
}