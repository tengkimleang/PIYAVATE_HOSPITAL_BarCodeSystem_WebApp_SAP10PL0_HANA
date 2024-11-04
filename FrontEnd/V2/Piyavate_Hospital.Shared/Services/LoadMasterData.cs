using System.Collections.ObjectModel;
using Piyavate_Hospital.Shared.Models;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.Shared
{
    // public class LoadMasterData(ApiService apiService) : ILoadMasterData
    // {
    //     readonly ApiService apiService = apiService;
    //     private ObservableCollection<Items> _getItems=new();
    //     private ObservableCollection<Vendors> _getVendors= new();
    //     private ObservableCollection<Vendors> _getCustomers= new();
    //     private ObservableCollection<ContactPersons> _getContactPersons = new();
    //     private ObservableCollection<VatGroups> _getTaxPurchases = new();
    //     private ObservableCollection<VatGroups> _getTaxSales = new();
    //     private ObservableCollection<Warehouses> _getWarehouses = new();
    //
    //     ObservableCollection<VatGroups> ILoadMasterData.GetTaxPurchases => _getTaxPurchases;
    //     ObservableCollection<VatGroups> ILoadMasterData.GetTaxSales => _getTaxSales;
    //     ObservableCollection<Warehouses> ILoadMasterData.GetWarehouses => _getWarehouses;
    //     ObservableCollection<Vendors> ILoadMasterData.GetCustomers => _getCustomers;
    //     ObservableCollection<Items> ILoadMasterData.GetItems => _getItems;
    //
    //     ObservableCollection<Vendors> ILoadMasterData.GetVendors => _getVendors;
    //     
    //
    //     ObservableCollection<ContactPersons> ILoadMasterData.GetContactPersons => _getContactPersons;
    //
    //     public async Task LoadContactPersonMaster()
    //     {
    //         var result = await apiService.GetContactPersons();
    //         if (result.ErrorCode == "")
    //         {
    //             _getContactPersons = new ObservableCollection<ContactPersons>(result.Data ?? new());
    //         }
    //     }
    //     public async Task LoadGetTaxSaleMaster()
    //     {
    //         var result = await apiService.GetTaxPurchases();
    //         if (result.ErrorCode == "")
    //         {
    //             _getTaxSales = new ObservableCollection<VatGroups>(result.Data ?? new());
    //         }
    //     }
    //     public async Task LoadGetTaxPurchaseMaster()
    //     {
    //         var result = await apiService.GetTaxPurchases();
    //         if (result.ErrorCode == "")
    //         {
    //             _getTaxPurchases = new ObservableCollection<VatGroups>(result.Data ?? new());
    //         }
    //     }
    //
    //     public async Task LoadGetWarehouseMaster()
    //     {
    //         var result = await apiService.GetContactPersons();
    //         if (result.ErrorCode == "")
    //         {
    //             _getContactPersons = new ObservableCollection<ContactPersons>(result.Data ?? new());
    //         }
    //     }
    //
    //     public async Task LoadItemMaster()
    //     {
    //         var result = await apiService.GetItems();
    //         if (result.ErrorCode == "")
    //         {
    //             _getItems = new ObservableCollection<Items>(result.Data ?? new());
    //         }
    //     }
    //
    //     public async Task LoadVendorMaster()
    //     {
    //         var result = await apiService.GetVendors();
    //         var customers = await apiService.GetCustomers();
    //
    //         if (result.ErrorCode == "")
    //         {
    //             _getVendors = result.Data?? new();
    //             _getCustomers = customers.Data?? new();
    //
    //         }
    //     }
    // }
}
