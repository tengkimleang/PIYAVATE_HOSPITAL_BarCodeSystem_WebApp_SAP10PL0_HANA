using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Piyavate_Hospital.Shared.Models;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.GoodReceiptPo;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.ViewModels;

public partial class GoodReceptPoViewModel(ApiService apiService) : ViewModelBase //, ILoadMasterData loadMasterData
{
    #region Data Member

    [ObservableProperty] GoodReceiptPoHeader _goodReceiptPoForm = new();

    [ObservableProperty] ObservableCollection<Series> _series = new();

    [ObservableProperty] private ObservableCollection<Vendors> _vendors = new(); //= loadMasterData.GetVendors;

    [ObservableProperty] private ObservableCollection<ContactPersons> _contactPeople = new(); //= loadMasterData.GetContactPersons;

    [ObservableProperty] private ObservableCollection<Items> _items = new(); //= loadMasterData.GetItems;

    [ObservableProperty] private ObservableCollection<VatGroups> _taxPurchases = new(); //= loadMasterData.GetTaxPurchases;

    [ObservableProperty] private ObservableCollection<Warehouses> _warehouses = new(); //= loadMasterData.GetWarehouses;

    [ObservableProperty] PostResponse _postResponses = new();

    [ObservableProperty] ObservableCollection<TotalItemCount> _totalItemCount = new();

    [ObservableProperty] ObservableCollection<TotalItemCount> _totalItemCountPurchaseOrder = new();

    [ObservableProperty] ObservableCollection<GetListData> _getListData = new();

    [ObservableProperty]
    ObservableCollection<GoodReceiptPoHeaderDeatialByDocNum> _goodReceiptPoHeaderDetailByDocNums = new();

    [ObservableProperty] ObservableCollection<GoodReceiptPoLineByDocNum> _goodReceiptPoLineByDocNums = new();

    [ObservableProperty] ObservableCollection<GetBatchOrSerial> _getBatchOrSerials = new();

    [ObservableProperty] ObservableCollection<GoodReceiptPoLineByDocNum> _getPurchaseOrderLineByDocNums = new();

    [ObservableProperty] ObservableCollection<GetGennerateBatchSerial> _getGenerateBatchSerial = new();

    [ObservableProperty] Boolean _isView;
    [ObservableProperty] string _token = string.Empty;
    [ObservableProperty] ObservableCollection<GetLayout> _getLayouts = new();
    #endregion

    #region Method
    [RelayCommand]
    async Task OnLoading()
    {
        Series = await CheckingValueT(Series, async () =>
            (await apiService.GetSeries("20",Token)).Data ?? new());
        GoodReceiptPoForm.Series = Series.First().Code;
        Vendors = await CheckingValueT(Vendors, async () =>
            (await apiService.GetVendors(Token)).Data ?? new());
        ContactPeople = await CheckingValueT(ContactPeople, async () =>
            (await apiService.GetContactPersons(Token)).Data ?? new());
        Items = await CheckingValueT(Items, async () =>
            (await apiService.GetItems(Token)).Data ?? new());
        TaxPurchases = await CheckingValueT(TaxPurchases, async () =>
            (await apiService.GetTaxPurchases(Token)).Data ?? new());
        Warehouses = await CheckingValueT(Warehouses, async () =>
            (await apiService.GetWarehouses(Token)).Data ?? new());
        IsView = true;
    }

    [RelayCommand]
    async Task TotalCountPurchaseOrder()
    {
        TotalItemCountPurchaseOrder = (await apiService.GetTotalItemCount("PurchaseOrder",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task TotalCountGoodReceiptPo()
    {
        TotalItemCount = (await apiService.GetTotalItemCount("GoodReceiptPo",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task Submit()
    {
        GoodReceiptPoForm.ContactPersonCode = string.IsNullOrEmpty(GoodReceiptPoForm.ContactPersonCode)
            ? "0"
            : GoodReceiptPoForm.ContactPersonCode;
        PostResponses = await apiService.PostGoodReceptPo(GoodReceiptPoForm,Token);
    }

    [RelayCommand]
    async Task OnGetGoodReceiptPo(string perPage)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("GoodReceiptPoHeader", perPage,Token)).Data ?? new();
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
            GetListData = (await apiService.GetListGoodReceiptPo("GoodReceiptPoHeader", ""
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
    async Task OnGetGoodReceiptPoHeaderDeatialByDocNum(string docEntry)
    {
        GoodReceiptPoHeaderDetailByDocNums =
            (await apiService.GoodReceiptPoHeaderDeatialByDocNum(docEntry,
                "GET_GoodReceipt_PO_Header_Detail_By_DocNum",Token)).Data ?? new();
        GoodReceiptPoLineByDocNums =
            (await apiService.GetLineByDocNum("GET_GoodReceipt_PO_Line_Detail_By_DocNum", docEntry,Token)).Data ?? new();
        GetBatchOrSerials = (await apiService.GetBatchOrSerial(docEntry, "GetBatchSerialGoodReceipt",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task OnGetPurchaseOrderLineByDocNum(string docEntry)
    {
        GoodReceiptPoHeaderDetailByDocNums =
            (await apiService.GoodReceiptPoHeaderDeatialByDocNum(docEntry,
                "GET_PurchaseOrder_Header_Detail_By_DocNum",Token)).Data ?? new();
        GetPurchaseOrderLineByDocNums =
            (await apiService.GetLineByDocNum("GET_PurchaseOrder_Line_Detail_By_DocNum", docEntry,Token)).Data ?? new();
    }

    [RelayCommand]
    async Task OnGetGennerateBatchSerial(Dictionary<string, object> data)
    {
        try
        {
            GetGenerateBatchSerial = (await apiService.GennerateBatchSerial(data["itemCode"].ToString() ?? ""
                , data["qty"].ToString() ?? "",Token)).Data ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    [RelayCommand]
    async Task OnGetLayoutPrint()
    {
        try
        {
            GetLayouts = (await apiService.GetLayoutPrinter("GOODRECEIPTPO", Token)).Data ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    #endregion
}