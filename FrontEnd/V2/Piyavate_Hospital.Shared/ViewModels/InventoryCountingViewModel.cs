using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Piyavate_Hospital.Shared.Models;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.InventoryCounting;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.ViewModels;

public partial class InventoryCountingViewModel(ApiService apiService) : ViewModelBase //, ILoadMasterData loadMasterData
{
     #region Data Member

    [ObservableProperty] InventoryCountingHeader _inventoryCountingHeader = new();
    
    [ObservableProperty] ObservableCollection<InventoryCountingLine> _inventoryCountingLines = new();

    [ObservableProperty] PostResponse _postResponses = new();

    [ObservableProperty] ObservableCollection<TotalItemCount> _totalItemCount = new();

    [ObservableProperty] ObservableCollection<GetListData> _getListData = new();

    [ObservableProperty] ObservableCollection<GetInventoryCountingList> _getInventoryCountingLists = new();
    
    [ObservableProperty] ObservableCollection<GetInventoryCountingLines> _getInventoryCountingLines = new();

    [ObservableProperty] Boolean _isView;

    [ObservableProperty] private ObservableCollection<Warehouses> _warehouses = new(); //= loadMasterData.GetWarehouses;

    [ObservableProperty] ObservableCollection<GetBatchOrSerial> _getBatchOrSerialsByItemCode = new();

    [ObservableProperty] ObservableCollection<GetBatchOrSerial> _getBatchOrSerials = new();

    [ObservableProperty] ObservableCollection<GetDetailInventoryCountingHeaderByDocNum> _getDetailInventoryCountingHeaderByDocNums = new();

    [ObservableProperty]  ObservableCollection<GetDetailInventoryCountingLineByDocNum> _getDetailInventoryCountingLineByDocNums = new();
    [ObservableProperty] string _token = string.Empty;
    #endregion 

    #region Method

    [RelayCommand]
    async Task OnLoading()
    {
        GetInventoryCountingLists = await CheckingValueT(GetInventoryCountingLists, async () =>
            (await apiService.GetInventoryCountingLists("GetInventoryCountingList",Token)).Data ?? new());
        Warehouses = await CheckingValueT(Warehouses, async () =>
            (await apiService.GetWarehouses(Token)).Data ?? new());
        await TotalCountInventoryCounting();
        IsView = true;
    }

    [RelayCommand]
    async Task TotalCountInventoryCounting()
    {
        TotalItemCount = (await apiService.GetTotalItemCount("InventoryCounting",Token)).Data ?? new();
    }
    [RelayCommand]
    async Task Submit()
    {
        PostResponses = await apiService.PostInventoryCounting(InventoryCountingHeader,Token);
    }

    [RelayCommand]
    async Task OnGetGoodReceiptPo(string perPage)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("InventoryCounting", perPage,Token)).Data ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [RelayCommand]
    async Task OnGetGoodReceiptPoBySearch(Dictionary<string, object> data)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("InventoryCounting", ""
                ,Token
                , "condition"
                , data["dateFrom"].ToString() ?? ""
                , data["dateTo"].ToString() ?? ""
                , data["docNum"].ToString() ?? "")).Data ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [RelayCommand]
    async Task OnGetPurchaseOrder(string perPage)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("GET_PURCHASE_ORDER", perPage,Token)).Data ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [RelayCommand]
    async Task OnGetPurchaseOrderBySearch(Dictionary<string, object> data)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("GET_PURCHASE_ORDER", ""
                , "condition"
                , data["dateFrom"].ToString() ?? ""
                , data["dateTo"].ToString() ?? ""
                , data["docNum"].ToString() ?? "")).Data ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    [RelayCommand]
    async Task OnGetPurchaseOrderLineByDocEntry(string docEntry)
    {
        try
        {
            GetInventoryCountingLines = new();
            GetInventoryCountingLines = (await apiService.GetInventoryCountingLines(docEntry,Token)).Data ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    [RelayCommand]
    async Task OnGetBatchOrSerialByItemCode(Dictionary<string,string> dictionary)
    {
        try
        {
            GetBatchOrSerialsByItemCode = (await apiService.GetBatchOrSerialByItemCode("OnGetBatchOrSerialAvailableByItemCode", dictionary["ItemType"],dictionary["ItemCode"],Token)).Data ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    [RelayCommand]
    async Task OnIssueForProductionDeatialByDocNum(string docEntry)
    {
        GetDetailInventoryCountingHeaderByDocNums = (await apiService.GetDetailInventoryCountingHeaderByDocNum(docEntry,Token)).Data ?? new();
        GetDetailInventoryCountingLineByDocNums = (await apiService.GetDetailInventoryCountingLineByDocNum(docEntry,Token)).Data ?? new();
        GetBatchOrSerials = (await apiService.GetBatchOrSerial(docEntry, "GetBatchSerialInventoryCounting",Token)).Data ?? new();
    }
    #endregion
}