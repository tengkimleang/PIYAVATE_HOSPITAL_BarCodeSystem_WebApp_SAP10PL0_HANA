using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Piyavate_Hospital.Shared.Models;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.ReturnComponentProduction;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.ViewModels;

public partial class ReturnFromComponentViewModel(ApiService apiService) : ViewModelBase //, ILoadMasterData loadMasterData
{
    #region Data Member

    [ObservableProperty] ReturnComponentProductionHeader _receiptFromProductionOrderForm = new();

    [ObservableProperty] ObservableCollection<ReturnComponentProductionLine> _issueProductionLine = new();

    [ObservableProperty] ObservableCollection<Series> _series = new();

    [ObservableProperty] PostResponse _postResponses = new();

    [ObservableProperty] ObservableCollection<TotalItemCount> _totalItemCount = new();

    [ObservableProperty] ObservableCollection<GetListData> _getListData = new();

    [ObservableProperty] ObservableCollection<GetProductionOrder> _getProductionOrder = new();

    [ObservableProperty] ObservableCollection<GetProductionOrderLines> _getProductionOrderLines = new();

    [ObservableProperty] Boolean _isView;

    [ObservableProperty] private ObservableCollection<Warehouses> _warehouses = new(); //= loadMasterData.GetWarehouses;
    [ObservableProperty] ObservableCollection<GetBatchOrSerial> _getBatchOrSerialsByItemCode = new();
    [ObservableProperty]
    ObservableCollection<GetBatchOrSerial> _getBatchOrSerials = new();
    [ObservableProperty]
    ObservableCollection<GoodReceiptPoHeaderDeatialByDocNum> _goodReceiptPoHeaderDetailByDocNums = new();
    [ObservableProperty]
    ObservableCollection<GoodReceiptPoLineByDocNum> _goodReceiptPoLineByDocNums = new();
    [ObservableProperty] string _token = string.Empty;
    [ObservableProperty] ObservableCollection<GetLayout> _getLayouts = new();
    #endregion 

    #region Method

    public override async Task Loaded()
    {
        Series = await CheckingValueT(Series, async () =>
            (await apiService.GetSeries("59",Token)).Data ?? new());
        GetProductionOrder = await CheckingValueT(GetProductionOrder, async () =>
            (await apiService.GetProductionOrders("GetForReceiptProduction",Token)).Data ?? new());
        Warehouses = await CheckingValueT(Warehouses, async () =>
            (await apiService.GetWarehouses(Token)).Data ?? new());
        ReceiptFromProductionOrderForm.Series = Series.First().Code;
        await TotalCountReceiptFromProduction();
        IsView = true;
    }

    [RelayCommand]
    async Task TotalCountReceiptFromProduction()
    {
        TotalItemCount = (await apiService.GetTotalItemCount("ReceiptFromProduction",Token)).Data ?? new();
    }
    [RelayCommand]
    async Task Submit()
    {
        PostResponses = await apiService.PostReturnFromProduction(ReceiptFromProductionOrderForm,Token);
    }
    [RelayCommand]
    async Task OnGetProductionOrder()
    {
        GetProductionOrder = (await apiService.GetProductionOrders("GetForReceiptProduction",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task OnGetGoodReceiptPo(string perPage)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("ReceiptForProduction", perPage,Token)).Data ?? new();
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
            GetListData = (await apiService.GetListGoodReceiptPo("ReceiptForProduction", ""
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
    async Task OnGetPurchaseOrderLineByDocEntry(string docEntry)
    {
        try
        {
            GetProductionOrderLines = new();
            GetProductionOrderLines = (await apiService.GetIssueProductionLines(docEntry,Token)).Data ?? new();
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
            //todo
            GetBatchOrSerialsByItemCode = (await apiService.GetBatchOrSerialByItemCode("OnGetBatchOrSerialInIssueForProduction",
                dictionary["ItemType"],
                dictionary["ItemCode"],
                Token,
                dictionary["DocEntry"])).Data ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    [RelayCommand]
    async Task OnIssueForProductionDetailByDocNum(string docEntry)
    {
        GoodReceiptPoHeaderDetailByDocNums = (await apiService.GoodReceiptPoHeaderDeatialByDocNum(docEntry,
            "GET_ReceiptForProduction_Header_Detail_By_DocNum",Token)).Data ?? new();
        GoodReceiptPoLineByDocNums = (await apiService.GetLineByDocNum("GetReceiptForProductionLineDetailByDocEntry",
            docEntry,Token)).Data ?? new();
        GetBatchOrSerials = (await apiService.GetBatchOrSerial(docEntry,
            "GetBatchSerialReceiptForProduction",Token)).Data ?? new();
    }
    [RelayCommand]
    async Task OnGetLayoutPrint()
    {
        try
        {
            GetLayouts = (await apiService.GetLayoutPrinter("RETURNCOMPONENT", Token)).Data ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    #endregion
}