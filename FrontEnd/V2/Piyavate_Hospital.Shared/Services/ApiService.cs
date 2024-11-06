using System.Collections.ObjectModel;
using System.Net;
using Piyavate_Hospital.Shared.Models;
using Piyavate_Hospital.Shared.Models.DeliveryOrder;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.GoodReceiptPo;
using Piyavate_Hospital.Shared.Models.InventoryCounting;
using Piyavate_Hospital.Shared.Models.InventoryTransfer;
using Piyavate_Hospital.Shared.Models.IssueForProduction;
using Piyavate_Hospital.Shared.Models.ProductionProcess;
using Piyavate_Hospital.Shared.Models.ReceiptFinishGood;
using Piyavate_Hospital.Shared.Models.ReturnComponentProduction;
using Piyavate_Hospital.Shared.Models.User;
using Piyavate_Hospital.Shared.ViewModels;

namespace Piyavate_Hospital.Shared.Services;

public class ApiService(IApiService apiService)
{
    public Task<ResponseData<ObservableCollection<Series>>> GetSeries(string seriesNumber, string token)
        => apiService.GetSeries(new GetRequest(
            ApiConstant.StoreProcedure, "SERIES", seriesNumber), token);

    public Task<ResponseData<ObservableCollection<Items>>> GetItems(string token)
        => apiService.GetItems(new GetRequest(
            ApiConstant.StoreProcedure, "GetItem"), token);

    public Task<ResponseData<ObservableCollection<Vendors>>> GetVendors(string token)
        => apiService.GetVendors(new GetRequest(
            ApiConstant.StoreProcedure, "GetVendor"), token);

    public Task<ResponseData<ObservableCollection<Vendors>>> GetCustomers(string token)
        => apiService.GetVendors(new GetRequest(
            ApiConstant.StoreProcedure, "GetCustomer"), token);

    public Task<ResponseData<ObservableCollection<ContactPersons>>> GetContactPersons(string token)
        => apiService.GetContactPersons(new GetRequest(
            ApiConstant.StoreProcedure, "GetContactPersonByCardCode"), token);

    public Task<ResponseData<ObservableCollection<VatGroups>>> GetTaxPurchases(string token)
        => apiService.GetTaxPurchases(new GetRequest(
            ApiConstant.StoreProcedure, "GetVatCodePurchase"), token);

    public Task<ResponseData<ObservableCollection<VatGroups>>> GetTaxSales(string token)
        => apiService.GetTaxSales(new GetRequest(
            ApiConstant.StoreProcedure, "GetTaxSale"), token);

    public Task<ResponseData<ObservableCollection<Warehouses>>> GetWarehouses(string token)
        => apiService.GetWarehouses(new GetRequest(
            ApiConstant.StoreProcedure, "GetWarehouseMasterData"), token);

    public Task<ResponseData<ObservableCollection<TotalItemCount>>> GetTotalItemCount(string type, string token)
        => apiService.GetTotalItemCount(new GetRequest(
            ApiConstant.StoreProcedure, "TotalItemCount", type), token);

    public Task<ResponseData<ObservableCollection<GetListData>>> GetListGoodReceiptPo(string storeType, string perPage
        , string token, string type = "", string dateFrom = "", string dateTo = "", string docNum = "")
        => apiService.GetListGoodReceiptPo(new GetRequest(
            ApiConstant.StoreProcedure, storeType, perPage, type, dateFrom, dateTo, docNum), token);

    public Task<PostResponse> PostGoodReceptPo(GoodReceiptPoHeader goodReceiptPoHeader, string token)
        => apiService.PostGoodReceptPo(goodReceiptPoHeader, token);

    public Task<PostResponse> PostDelveryOrder(DeliveryOrderHeader deliveryOrderHeader, string token)
        => apiService.PostDelveryOrder(deliveryOrderHeader, token);

    public Task<ResponseData<ObservableCollection<GoodReceiptPoHeaderDeatialByDocNum>>>
        GoodReceiptPoHeaderDeatialByDocNum(string docEntry, string type, string token)
        => apiService.GoodReceiptPoHeaderDeatialByDocNum(new GetRequest(
            ApiConstant.StoreProcedure, type, docEntry), token);

    public Task<ResponseData<ObservableCollection<GoodReceiptPoLineByDocNum>>> GetLineByDocNum(string storeType,
        string docEntry, string token)
        => apiService.GetLineByDocNum(new GetRequest(
            ApiConstant.StoreProcedure, storeType, docEntry), token);

    public Task<ResponseData<ObservableCollection<GetBatchOrSerial>>> GetBatchOrSerial(string docEntry, string type,
        string token,
        string lineNum = "")
        => apiService.GetBatchOrSerial(new GetRequest(
            ApiConstant.StoreProcedure, type, docEntry, lineNum), token);

    public Task<ResponseData<ObservableCollection<GetBatchOrSerial>>> GetBatchOrSerialByItemCode(string storeType,
        string itemType, string itemCode, string token, string docEntry = "")
        => apiService.GetBatchOrSerial(new GetRequest(
            ApiConstant.StoreProcedure, storeType, itemType, itemCode, docEntry), token);

    public Task<ResponseData<ObservableCollection<GetGennerateBatchSerial>>> GennerateBatchSerial(string itemCode,
        string qty, string token)
        => apiService.GennerateBatchSerial(new GetRequest(
            ApiConstant.StoreProcedure, "GennerateBatchOrSerial", itemCode, qty), token);

    public Task<ResponseData<ObservableCollection<GetProductionOrder>>> GetProductionOrders(string type, string token)
        => apiService.GetProductionOrders(new GetRequest(
            ApiConstant.StoreProcedure, "GET_Production_Order", type), token);

    public Task<ResponseData<ObservableCollection<GetProductionOrderLines>>> GetProductionOrderLines(string docEntry,
        string token)
        => apiService.GetProductionOrderLines(new GetRequest(
            ApiConstant.StoreProcedure, "GET_Production_Order_Lines", docEntry), token);

    public Task<PostResponse> PostIssueProduction(IssueProductionHeader issueProductionHeader, string token)
        => apiService.PostIssueProduction(issueProductionHeader, token);

    public Task<ResponseData<ObservableCollection<GetProductionOrderLines>>> GetIssueProductionLines(string docEntry,
        string token)
        => apiService.GetProductionOrderLines(new GetRequest(
            ApiConstant.StoreProcedure, "GET_Issue_Production_Lines", docEntry), token);

    public Task<ResponseData<ObservableCollection<GetInventoryCountingList>>> GetInventoryCountingLists(string type,
        string token)
        => apiService.GetInventoryCountingLists(new GetRequest(
            ApiConstant.StoreProcedure, "GetInventoryCountingList", type), token);

    public Task<ResponseData<ObservableCollection<GetInventoryCountingLines>>> GetInventoryCountingLines(
        string docEntry, string token)
        => apiService.GetInventoryCountingLines(new GetRequest(
            ApiConstant.StoreProcedure, "GetInventoryCountingLine", docEntry), token);

    public Task<PostResponse> PostReturnFromProduction(ReturnComponentProductionHeader issueProductionHeader,
        string token)
        => apiService.PostReturnFromProduction(issueProductionHeader, token);

    public Task<PostResponse> PostInventoryTransfer(InventoryTransferHeader inventoryTransfer, string token)
        => apiService.PostInventoryTransfer(inventoryTransfer, token);

    public Task<PostResponse> PostReturn(DeliveryOrderHeader deliveryOrderHeader, string token)
        => apiService.PostReturn(deliveryOrderHeader, token);

    public Task<PostResponse> PostReturnRequest(DeliveryOrderHeader deliveryOrderHeader, string token)
        => apiService.PostReturnRequest(deliveryOrderHeader, token);

    public Task<PostResponse> PostGoodReturn(DeliveryOrderHeader deliveryOrderHeader, string token)
        => apiService.PostGoodReturn(deliveryOrderHeader, token);

    public Task<PostResponse> PostArCreditMemo(DeliveryOrderHeader deliveryOrderHeader, string token)
        => apiService.PostArCreditMemo(deliveryOrderHeader, token);

    public Task<PostResponse> PostInventoryCounting(InventoryCountingHeader deliveryOrderHeader, string token)
        => apiService.PostInventoryCounting(deliveryOrderHeader, token);

    public Task<ResponseData<ObservableCollection<GetDetailInventoryCountingHeaderByDocNum>>>
        GetDetailInventoryCountingHeaderByDocNum(string docEntry, string token)
        => apiService.GetDetailInventoryCountingHeaderByDocNum(new GetRequest(
            ApiConstant.StoreProcedure, "GET_InventoryCounting_Header_Detail_By_DocNum", docEntry), token);

    public Task<ResponseData<ObservableCollection<GetDetailInventoryCountingLineByDocNum>>>
        GetDetailInventoryCountingLineByDocNum(string docEntry, string token)
        => apiService.GetDetailInventoryCountingLineByDocNum(new GetRequest(
            ApiConstant.StoreProcedure, "GetInventoryCountingLineDetailByDocEntry", docEntry), token);

    public Task<PostResponse> PostProductionProcess(ProductionProcessHeader productionProcessHeader, string token)
        => apiService.UpdateProcessProduction(productionProcessHeader, token);

    public Task<ResponseData<ObservableCollection<GetProductionOrderLines>>> GetProductionFinishedGoodLines(
        string docEntry, string token)
        => apiService.GetProductionOrderLines(new GetRequest(
            ApiConstant.StoreProcedure, "GET_Production_Finished_Good", docEntry), token);

    public Task<PostResponse> PostReceiptFinishGood(ReceiptFinishGoodHeader receiptFinishGoodHeader, string token)
        => apiService.PostReceiptFinishGood(receiptFinishGoodHeader, token);

    public Task<ResponseData<ObservableCollection<GetLayout>>> GetLayoutPrinter(string layoutType, string token)
        => apiService.GetLayoutPrinter(new GetRequest(
            ApiConstant.StoreProcedure, "LayoutPrinter", layoutType), token);

    public Task<CheckUserResponse> CheckingUser(string userName, string password)
        => apiService.CheckingUser(new CreateUser { Account = userName, Password = password });
}