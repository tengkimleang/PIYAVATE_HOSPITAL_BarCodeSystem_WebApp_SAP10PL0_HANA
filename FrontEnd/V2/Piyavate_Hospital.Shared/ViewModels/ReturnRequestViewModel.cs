using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Piyavate_Hospital.Shared.Models;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.ViewModels;

public partial class ReturnRequestViewModel(ApiService apiService) : ViewModelBase //, ILoadMasterData loadMasterData
{
    #region Data Member

    [ObservableProperty] DeliveryOrderHeader _deliveryOrderForm = new();

    [ObservableProperty] ObservableCollection<Series> _series = new();

    [ObservableProperty] private ObservableCollection<Vendors> _customers = new(); //= loadMasterData.GetCustomers;

    [ObservableProperty] private ObservableCollection<ContactPersons> _contactPeople = new(); //= loadMasterData.GetContactPersons;

    [ObservableProperty] private ObservableCollection<Items> _items = new(); //= loadMasterData.GetItems;

    [ObservableProperty] private ObservableCollection<VatGroups> _taxSales = new(); //= loadMasterData.GetTaxSales;

    [ObservableProperty] private ObservableCollection<Warehouses> _warehouses = new(); //= loadMasterData.GetWarehouses;

    [ObservableProperty] PostResponse _postResponses = new();

    [ObservableProperty] ObservableCollection<TotalItemCount> _totalItemCount = new();

    [ObservableProperty] ObservableCollection<TotalItemCount> _totalItemCountDeliveryOrder = new();

    [ObservableProperty] ObservableCollection<GetListData> _getListData = new();

    [ObservableProperty]
    ObservableCollection<GoodReceiptPoHeaderDeatialByDocNum> _goodReceiptPoHeaderDetailByDocNums = new();

    [ObservableProperty] ObservableCollection<GoodReceiptPoLineByDocNum> _goodReceiptPoLineByDocNums = new();

    [ObservableProperty] ObservableCollection<GetBatchOrSerial> _getBatchOrSerials = new();

    [ObservableProperty] ObservableCollection<GoodReceiptPoLineByDocNum> _getPurchaseOrderLineByDocNums = new();

    [ObservableProperty] ObservableCollection<GetBatchOrSerial> _getBatchOrSerialsByItemCode = new();

    [ObservableProperty] bool _isView;
    [ObservableProperty] string _token = string.Empty;
    #endregion

    #region Method
    [RelayCommand]
    async Task OnLoading()
    {
        Series = await CheckingValueT(Series, async () =>
            (await apiService.GetSeries("234000031",Token)).Data ?? new());
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
        // await TotalCountReturn();
        // await TotalCountDeliveryOrderReturn();
        IsView = true;
    }
    [RelayCommand]
    async Task TotalCountDeliveryOrderReturn()
    {
        TotalItemCountDeliveryOrder = (await apiService.GetTotalItemCount("DeliveryOrderReturnRequest",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task TotalCountReturn()
    {
        TotalItemCount = (await apiService.GetTotalItemCount("ReturnRequest",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task Submit()
    {
        DeliveryOrderForm.ContactPersonCode = string.IsNullOrEmpty(DeliveryOrderForm.ContactPersonCode)
            ? "0"
            : DeliveryOrderForm.ContactPersonCode;
        PostResponses = await apiService.PostReturnRequest(DeliveryOrderForm,Token);
    }

    [RelayCommand]
    async Task OnGetGoodReceiptPo(string perPage)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("GetReturnRequestHeader", perPage,Token)).Data ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    [RelayCommand]
    async Task OnGetGoodReturnBySearch(Dictionary<string, object> data)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("GetReturnRequestHeader", ""
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
            GetListData = (await apiService.GetListGoodReceiptPo("GetDeliveryOrderReturnRequest", perPage,Token)).Data ??
                          new();
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
                "GET_Return_Request_Header_Detail_By_DocNum",Token))
            .Data ?? new();
        GoodReceiptPoLineByDocNums =
            (await apiService.GetLineByDocNum("GetReturnRequestLineDetailByDocEntry", docEntry,Token)).Data ?? new();
        GetBatchOrSerials = (await apiService.GetBatchOrSerial(docEntry, "GetBatchSerialReturnRequest",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task OnGetPurchaseOrderLineByDocNum(string docEntry)
    {
        GoodReceiptPoHeaderDetailByDocNums =
            (await apiService.GoodReceiptPoHeaderDeatialByDocNum(docEntry,
                "GET_Delivery_Order_Header_Detail_By_DocNum_Return_Request",Token))
            .Data ?? new();
        GetPurchaseOrderLineByDocNums =
            (await apiService.GetLineByDocNum("GetDeliveryOrderLineForReturnRequestDetailByDocEntry", docEntry,Token)).Data ??
            new();
        foreach (var obj in GetPurchaseOrderLineByDocNums)
        {
            if (obj.ManageItem == "S")
            {
                obj.Serials = new();
                var rs =
                    (await apiService.GetBatchOrSerial(docEntry, "GetBatchSerialDeliveryOrder",Token, obj.BaseLineNumber))
                    .Data ?? new();
                foreach (var objSerial in rs)
                {
                    obj.Serials.Add(new SerialGoodReceiptPoCopyFrom
                    {
                        MfrNo = objSerial.MfrSerialNo,
                        SerialCode = objSerial.SerialBatch,
                        Qty = Convert.ToInt32(objSerial.Qty),
                        MfrDate = (!string.IsNullOrEmpty(objSerial.MrfDate))
                            ? Convert.ToDateTime(objSerial.MrfDate)
                            : DateTime.Now,
                        ExpDate = (!string.IsNullOrEmpty(objSerial.ExpDate))
                            ? Convert.ToDateTime(objSerial.ExpDate)
                            : DateTime.Now,
                        OnSelectedBatchOrSerial = new[] { objSerial },
                    });
                }
            }
            else
            {
                obj.Batches = new();
                var rs =
                    (await apiService.GetBatchOrSerial(docEntry, "GetBatchSerialDeliveryOrder",Token, obj.BaseLineNumber))
                    .Data ?? new();
                foreach (var objBatch in rs)
                {
                    obj.Batches.Add(new BatchGoodReceiptPoCopyFrom
                    {
                        LotNo = objBatch.MfrSerialNo,
                        BatchCode = objBatch.SerialBatch,
                        Qty = Convert.ToDouble(objBatch.Qty),
                        ManfectureDate = (!string.IsNullOrEmpty(objBatch.MrfDate))
                            ? Convert.ToDateTime(objBatch.MrfDate)
                            : DateTime.Now,
                        ExpDate = (!string.IsNullOrEmpty(objBatch.ExpDate))
                            ? Convert.ToDateTime(objBatch.ExpDate)
                            : DateTime.Now,
                        QtyAvailable = Convert.ToDouble(objBatch.Qty),
                        OnSelectedBatchOrSerial = new[] { objBatch },
                    });
                }
            }
        }
    }

    [RelayCommand]
    async Task OnGetGoodReceiptPoBySearch(Dictionary<string, object> data)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("ReturnRequestDoHeader", ""
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