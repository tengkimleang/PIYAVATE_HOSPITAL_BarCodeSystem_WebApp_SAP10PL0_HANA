using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Piyavate_Hospital.Shared.Models;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Pages;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.ViewModels;

public partial class DeliveryOrderViewModel(
    ApiService apiService) : ViewModelBase
{
    #region Data Member

    [ObservableProperty] DeliveryOrderHeader _deliveryOrderForm = new();

    [ObservableProperty] ObservableCollection<Series> _series = new();

    [ObservableProperty] private ObservableCollection<Vendors> _customers = new(); //= loadMasterData.GetCustomers;

    [ObservableProperty]
    private ObservableCollection<ContactPersons> _contactPeople = new(); //= loadMasterData.GetContactPersons;

    [ObservableProperty] private ObservableCollection<Items> _items = new(); //= loadMasterData.GetItems;

    [ObservableProperty] private ObservableCollection<VatGroups> _taxSales = new(); //= loadMasterData.GetTaxSales;

    [ObservableProperty] private ObservableCollection<Warehouses> _warehouses = new(); //= loadMasterData.GetWarehouses;

    [ObservableProperty] PostResponse _postResponses = new();

    [ObservableProperty] ObservableCollection<TotalItemCount> _totalItemCount = new();

    [ObservableProperty] ObservableCollection<TotalItemCount> _totalItemCountSalesOrder = new();

    [ObservableProperty] ObservableCollection<GetListData> _getListData = new();

    [ObservableProperty]
    ObservableCollection<GoodReceiptPoHeaderDeatialByDocNum> _goodReceiptPoHeaderDeatialByDocNums = new();

    [ObservableProperty] ObservableCollection<GoodReceiptPoLineByDocNum> _goodReceiptPoLineByDocNums = new();

    [ObservableProperty] ObservableCollection<GetBatchOrSerial> _getBatchOrSerials = new();

    [ObservableProperty] ObservableCollection<GoodReceiptPoLineByDocNum> _getPurchaseOrderLineByDocNums = new();

    [ObservableProperty] ObservableCollection<GetBatchOrSerial> _getBatchOrSerialsByItemCode = new();

    [ObservableProperty] Boolean _isView = true;
    [ObservableProperty] string _token = string.Empty;

    #endregion

    #region Method

    // public override async Task Loaded()
    // {
    //     
    // }

    [RelayCommand]
    async Task OnLoading()
    {
        Series = await CheckingValueT(Series, async () =>
            (await apiService.GetSeries("15", Token)).Data ?? new());
        DeliveryOrderForm.Series = Series.First().Code;
        Customers = await CheckingValueT(Customers, async () =>
            (await apiService.GetCustomers(Token)).Data ?? new());
        ContactPeople = await CheckingValueT(ContactPeople, async () =>
            (await apiService.GetContactPersons(Token)).Data ?? new());
        Items = await CheckingValueT(Items, async () =>
            (await apiService.GetItems(Token)).Data ?? new());
        TaxSales = await CheckingValueT(TaxSales, async () =>
            (await apiService.GetTaxSales(Token)).Data ?? new());
        Warehouses = await CheckingValueT(Warehouses, async () =>
            (await apiService.GetWarehouses(Token)).Data ?? new());
        // await OnTotalItemCountDeliveryOrder();
        // await OnTotalItemCountSaleOrder();
        IsView = true;
    }

    [RelayCommand]
    async Task OnTotalItemCountDeliveryOrder()
    {
        TotalItemCount = (await apiService.GetTotalItemCount("DeliveryOrder",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task OnTotalItemCountSaleOrder()
    {
        TotalItemCountSalesOrder = (await apiService.GetTotalItemCount("SaleOrder",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task Submit()
    {
        DeliveryOrderForm.ContactPersonCode = string.IsNullOrEmpty(DeliveryOrderForm.ContactPersonCode)
            ? "0"
            : DeliveryOrderForm.ContactPersonCode;
        PostResponses = await apiService.PostDelveryOrder(DeliveryOrderForm,Token);
    }

    [RelayCommand]
    async Task OnGetGoodReceiptPo(string perPage)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("GetDeliveryOrderHeader", perPage,Token)).Data ?? new();
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
            GetBatchOrSerialsByItemCode =
                (await apiService.GetBatchOrSerialByItemCode("OnGetBatchOrSerialAvailableByItemCode",
                    dictionary["ItemType"], dictionary["ItemCode"],Token)).Data ?? new();
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
        GoodReceiptPoHeaderDeatialByDocNums =
            (await apiService.GoodReceiptPoHeaderDeatialByDocNum(docEntry, "GET_DeliveryOrder_Header_Detail_By_DocNum",Token))
            .Data ?? new();
        GoodReceiptPoLineByDocNums =
            (await apiService.GetLineByDocNum("GetDeliveryOrderLineDetailByDocEntry", docEntry,Token)).Data ?? new();
        GetBatchOrSerials = (await apiService.GetBatchOrSerial(docEntry, "GetBatchSerialDeliveryOrder",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task OnGetPurchaseOrderLineByDocNum(string docEntry)
    {
        GoodReceiptPoHeaderDeatialByDocNums =
            (await apiService.GoodReceiptPoHeaderDeatialByDocNum(docEntry, "GET_SaleOrder_Header_Detail_By_DocNum",Token))
            .Data ?? new();
        GetPurchaseOrderLineByDocNums =
            (await apiService.GetLineByDocNum("GetSaleOrderLineDetailByDocEntry", docEntry,Token)).Data ?? new();
    }

    [RelayCommand]
    async Task OnGetGoodReceiptPoBySearch(Dictionary<string, object> data)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("GetDeliveryOrderHeader", ""
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
    async Task OnGetSaleOrderBySearch(Dictionary<string, object> data)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("GetSaleOrder", ""
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

    #endregion
}