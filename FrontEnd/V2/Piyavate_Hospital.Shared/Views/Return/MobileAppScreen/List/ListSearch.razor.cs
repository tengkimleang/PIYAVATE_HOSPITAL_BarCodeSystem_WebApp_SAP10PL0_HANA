using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.Views.Return.MobileAppScreen.List;

public partial class ListSearch 
{
    private int _refreshCount;
    int _count;
    private string? _searchValue;
    private readonly ObservableCollection<GetListData> _scrollingData = new();
    private bool _isViewDetail;

    protected override async void OnInitialized()
    {
        StateHasChanged();
        ComponentAttribute.Title = "List Search";
        ComponentAttribute.Path = "/goodreturn";
        ComponentAttribute.IsBackButton = true;
        await OnRefreshAsync();
        _isViewDetail = false;
    }

    private void UpdateGridSize(GridItemSize size)
    {
        if (size != GridItemSize.Xs)
        {
            NavigationManager.NavigateTo("goodreturn");
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
            // tmpData= new ObservableCollection<GetListData>(scrollingData);
            // // You can also call an API here if the list is not local.
            // var results = ViewModel.GetListData
            //     .Where(item => item.DocumentNumber.Contains(_searchValue, StringComparison.OrdinalIgnoreCase) 
            //                    || item.VendorCode.Contains(_searchValue, StringComparison.OrdinalIgnoreCase) 
            //                    || item.DocDate.Contains(_searchValue, StringComparison.OrdinalIgnoreCase)
            //                    || item.Remarks.Contains(_searchValue, StringComparison.OrdinalIgnoreCase))
            //     .ToList();
            // scrollingData.Clear();
            // foreach (var result in results)
            // {
            //     scrollingData.Add(result);
            // }
        }
        else
        {
            await OnRefreshAsync().ConfigureAwait(false);
        }
    }

    public async Task<bool> OnRefreshAsync()
    {
        await ViewModel.TotalCountReturnCommand.ExecuteAsync(null).ConfigureAwait(false);
        if (Convert.ToInt32(ViewModel.TotalItemCount.FirstOrDefault()?.AllItem ?? "0") <= _count)
        {
            return false;
        }

        await ViewModel.TotalCountReturnCommand.ExecuteAsync(_refreshCount.ToString()).ConfigureAwait(false);
        foreach (var item in ViewModel.GetListData)
        {
            _scrollingData.Add(item);
        }

        _refreshCount++;
        _count = +_scrollingData.Count;
        StateHasChanged();
        return true;
    }

    private async Task OnClickCopy(string docEntry)
    {
        await ViewModel.GetGoodReceiptPoHeaderDeatialByDocNumCommand.ExecuteAsync(docEntry).ConfigureAwait(false);
        _isViewDetail = true;
    }

    private Task OnViewDetail()
    {
        _isViewDetail = false;
        StateHasChanged();
        return Task.CompletedTask;
    }
}