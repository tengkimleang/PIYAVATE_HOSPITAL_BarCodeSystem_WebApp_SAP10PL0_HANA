using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Piyavate_Hospital.Shared.Models;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.InventoryTransfer;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.ViewModels;

public partial class InventoryTransferViewModel(ApiService apiService) : ViewModelBase //, ILoadMasterData loadMasterData
{
    [ObservableProperty]
    InventoryTransferHeader _inventoryTransferForm = new();

    [ObservableProperty]
    ObservableCollection<Series> _series = new();

    [ObservableProperty] private ObservableCollection<Items> _items = new(); //= loadMasterData.GetItems;

    [ObservableProperty] private ObservableCollection<Warehouses> _warehouses = new(); //= loadMasterData.GetWarehouses;

    [ObservableProperty] private ObservableCollection<Warehouses> _warehousesTo = new(); //= loadMasterData.GetWarehouses;

    [ObservableProperty]
    PostResponse _postResponses = new();

    [ObservableProperty]
    ObservableCollection<TotalItemCount> _totalItemCount = new();

    [ObservableProperty]
    ObservableCollection<TotalItemCount> _totalItemCountSalesOrder = new();

    [ObservableProperty]
    ObservableCollection<GetListData> _getListData = new();

    [ObservableProperty]
    ObservableCollection<GoodReceiptPoHeaderDeatialByDocNum> _inventoryTransferHeaderDetailByDocNums = new();

    [ObservableProperty]
    ObservableCollection<GoodReceiptPoLineByDocNum> _inventoryTransferLineByDocNums = new();

    [ObservableProperty]
    ObservableCollection<GetBatchOrSerial> _getBatchOrSerials = new();

    [ObservableProperty]
    ObservableCollection<GoodReceiptPoLineByDocNum> _getPurchaseOrderLineByDocNums = new();

    [ObservableProperty]
    ObservableCollection<GetBatchOrSerial> _getBatchOrSerialsByItemCode = new();

    [ObservableProperty]
    Boolean _isView;
    [ObservableProperty] string _token = string.Empty;
    
    [RelayCommand]
    async Task OnLoading()
    {
        Series = await CheckingValueT(Series, async () =>
                 (await apiService.GetSeries("67",Token)).Data ?? new());
        InventoryTransferForm.Series= Series.First().Code ?? "";
        Items = await CheckingValueT(Items, async () =>
                    (await apiService.GetItems(Token)).Data ?? new());
        Warehouses = await CheckingValueT(Warehouses, async () =>
                    (await apiService.GetWarehouses(Token)).Data ?? new());
        WarehousesTo = await CheckingValueT(WarehousesTo, async () =>
                           (await apiService.GetWarehouses(Token)).Data ?? new());
        InventoryTransferForm.FromWarehouse = Warehouses.First().Code ?? "";
        InventoryTransferForm.ToWarehouse = WarehousesTo.First().Code ?? "";
        IsView = true;
    }

    [RelayCommand]
    async Task TotalCountInventoryTransferRequest()
    {
        TotalItemCountSalesOrder = (await apiService.GetTotalItemCount("InventoryTransferRequest",Token)).Data ?? new();
    }
    
    [RelayCommand]
    async Task TotalCountInventoryTransfer()
    {
        TotalItemCount = (await apiService.GetTotalItemCount("InventoryTransfer",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task Submit()
    {
        PostResponses = await apiService.PostInventoryTransfer(InventoryTransferForm,Token);
    }

    [RelayCommand]
    async Task OnGetGoodReceiptPo(string perPage)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("GetInventoryTransferHeader", perPage,Token)).Data ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    [RelayCommand]
    async Task OnGetBatchOrSerialByItemCode(Dictionary<string, string> dictionary)
    {
        try
        {
            GetBatchOrSerialsByItemCode = (await apiService.GetBatchOrSerialByItemCode("OnGetBatchOrSerialAvailableByItemCode", dictionary["ItemType"], dictionary["ItemCode"],Token)).Data ?? new();
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
            GetListData = (await apiService.GetListGoodReceiptPo("GetSaleOrder", perPage,Token)).Data ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [RelayCommand]
    async Task OnGetGoodReceiptPoHeaderDeatialByDocNum(string docEntry)
    {
        InventoryTransferHeaderDetailByDocNums = (await apiService.GoodReceiptPoHeaderDeatialByDocNum(docEntry, "GET_InventoryTransfer_Header_Detail_By_DocNum",Token)).Data ?? new();
        InventoryTransferLineByDocNums = (await apiService.GetLineByDocNum("GetInventoryTransferLineDetailByDocEntry", docEntry,Token)).Data ?? new();
        GetBatchOrSerials = (await apiService.GetBatchOrSerial(docEntry, "GetBatchSerialInventoryTransfer",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task OnGetPurchaseOrderLineByDocNum(string docEntry)
    {
        GetPurchaseOrderLineByDocNums = (await apiService.GetLineByDocNum("GetSaleOrderLineDetailByDocEntry", docEntry,Token)).Data ?? new();
    }
    [RelayCommand]
    async Task OnGetGoodReceiptPoBySearch(Dictionary<string, object> data)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("GetInventoryTransferHeader", ""
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
}
