using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Piyavate_Hospital.Shared.Models;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.ViewModels;

public partial class GoodReturnViewModel(ApiService apiService) : ViewModelBase //, ILoadMasterData loadMasterData
{
    #region Data Member

    [ObservableProperty] DeliveryOrderHeader _goodReturnForm = new();

    [ObservableProperty] ObservableCollection<Series> _series = new();

    [ObservableProperty] private ObservableCollection<Vendors> _vendors = new(); //= loadMasterData.GetVendors;

    [ObservableProperty] private ObservableCollection<ContactPersons> _contactPeople = new(); //= loadMasterData.GetContactPersons;

    [ObservableProperty] private ObservableCollection<Items> _items = new(); //= loadMasterData.GetItems;

    [ObservableProperty] private ObservableCollection<VatGroups> _taxPurchases = new(); //= loadMasterData.GetTaxPurchases;

    [ObservableProperty] private ObservableCollection<Warehouses> _warehouses = new(); //= loadMasterData.GetWarehouses;

    [ObservableProperty] PostResponse _postResponses = new();

    [ObservableProperty] ObservableCollection<TotalItemCount> _totalItemCount = new();

    [ObservableProperty] ObservableCollection<TotalItemCount> _totalCountGoodReceiptPo = new();

    [ObservableProperty] ObservableCollection<GetListData> _getListData = new();

    [ObservableProperty]
    ObservableCollection<GoodReceiptPoHeaderDeatialByDocNum> _goodReturnHeaderDetailByDocNums = new();

    [ObservableProperty] ObservableCollection<GoodReceiptPoLineByDocNum> _goodReturnLineByDocNums = new();

    [ObservableProperty] ObservableCollection<GetBatchOrSerial> _getBatchOrSerials = new();

    [ObservableProperty] ObservableCollection<GoodReceiptPoLineByDocNum> _getPurchaseOrderLineByDocNums = new();

    [ObservableProperty] ObservableCollection<GetBatchOrSerial> _getBatchOrSerialsByItemCode = new();

    [ObservableProperty] Boolean _isView;
    [ObservableProperty] string _token = string.Empty;
    #endregion

    #region Method
    
    [RelayCommand]
    async Task OnLoading()
    {
        Series = await CheckingValueT(Series, async () =>
            (await apiService.GetSeries("21",Token)).Data ?? new());
        GoodReturnForm.Series = Series.First().Code;
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
    async Task TotalCountGoodReceiptPoReturn()
    {
        TotalCountGoodReceiptPo = (await apiService.GetTotalItemCount("GoodReceiptPOReturn",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task TotalCountGoodReturn()
    {
        TotalItemCount = (await apiService.GetTotalItemCount("GoodReturn",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task Submit()
    {
        GoodReturnForm.ContactPersonCode = string.IsNullOrEmpty(GoodReturnForm.ContactPersonCode)
            ? "0"
            : GoodReturnForm.ContactPersonCode;
        PostResponses = await apiService.PostGoodReturn(GoodReturnForm,Token);
    }

    [RelayCommand]
    async Task OnGetGoodReturn(string perPage)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("GetGoodReturnHeader", perPage,Token)).Data ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [RelayCommand]
    async Task OnGetReturnBySearch(Dictionary<string, object> data)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("GetGoodReturnHeader", ""
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
            GetBatchOrSerialsByItemCode = (
                await apiService.GetBatchOrSerialByItemCode(
                    "OnGetBatchOrSerialAvailableByItemCode",
                    dictionary["ItemType"],
                    dictionary["ItemCode"],Token)
            ).Data ?? new();
            //if(dictionary["DocEntry"]=="" && dictionary["DocEntry"] == "0")
            //{
            //}
            //else
            //{
            //    GetBatchOrSerialsByItemCode = (await apiService.GetBatchOrSerialByItemCode(
            //        "OnGetBatchOrSerialByItemCodeReuturnDelivery", 
            //        dictionary["ItemType"], 
            //        dictionary["ItemCode"], 
            //        dictionary["DocEntry"])).Data ?? new();
            //}
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [RelayCommand]
    async Task OnGetGoodReceiptPo(string perPage)
    {
        try
        {
            GetListData = (await apiService.GetListGoodReceiptPo("GoodReceiptPOHeaderByReturn", perPage,Token)).Data ?? new();
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
        GoodReturnHeaderDetailByDocNums =
            (await apiService.GoodReceiptPoHeaderDeatialByDocNum(docEntry, "GET_Good_Return_Header_Detail_By_DocNum",Token))
            .Data ?? new();
        GoodReturnLineByDocNums =
            (await apiService.GetLineByDocNum("GetGoodReturnLineDetailByDocEntry", docEntry,Token)).Data ?? new();
        GetBatchOrSerials = (await apiService.GetBatchOrSerial(docEntry, "GetBatchSerialGoodReturn",Token)).Data ?? new();
    }

    [RelayCommand]
    async Task OnGetPurchaseOrderLineByDocNum(string docEntry)
    {
        GoodReturnHeaderDetailByDocNums =
            (await apiService.GoodReceiptPoHeaderDeatialByDocNum(docEntry, "GoodReceiptPoHeaderReturnByDocEntry",Token))
            .Data ?? new();
        GetPurchaseOrderLineByDocNums =
            (await apiService.GetLineByDocNum("GetGoodReceiptPOLineForGoodReturnDetailByDocEntry", docEntry,Token)).Data ??
            new();
        foreach (var obj in GetPurchaseOrderLineByDocNums)
        {
            if (obj.ManageItem == "S")
            {
                obj.Serials = new();
                var rs =
                    (await apiService.GetBatchOrSerial(docEntry, "GetBatchSerialGoodReceiptPOForGoodReturn"
                        ,Token
                        ,obj.BaseLineNumber))
                    .Data ?? new();
                foreach (var objSerial in rs)
                {
                    obj.Serials.Add(new SerialGoodReceiptPoCopyFrom
                    {
                        MfrNo = objSerial.MfrSerialNo,
                        SerialCode = objSerial.SerialBatch,
                        Qty = Convert.ToInt32(objSerial.Qty),
                        MfrDate = (string.IsNullOrEmpty(objSerial.MrfDate))
                            ? DateTime.Now
                            : Convert.ToDateTime(objSerial.MrfDate),
                        ExpDate = (string.IsNullOrEmpty(objSerial.ExpDate))
                            ? DateTime.Now
                            : Convert.ToDateTime(objSerial.ExpDate),
                        OnSelectedBatchOrSerial = new[] { objSerial },
                    });
                }
            }
            else
            {
                obj.Batches = new();
                var rs =
                    (await apiService.GetBatchOrSerial(docEntry, "GetBatchSerialGoodReceiptPOForGoodReturn"
                        ,Token
                        ,obj.BaseLineNumber))
                    .Data ?? new();
                foreach (var objBatch in rs)
                {
                    obj.Batches.Add(new BatchGoodReceiptPoCopyFrom
                    {
                        LotNo = objBatch.MfrSerialNo,
                        BatchCode = objBatch.SerialBatch,
                        Qty = Convert.ToDouble(objBatch.Qty),
                        ManfectureDate = Convert.ToDateTime(objBatch.MrfDate ?? ""),
                        ExpDate = Convert.ToDateTime(objBatch.ExpDate ?? ""),
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
            GetListData = (await apiService.GetListGoodReceiptPo("GoodReceiptPOHeaderByReturn", ""
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