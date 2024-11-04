using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Piyavate_Hospital.Shared.Models;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Models.ProductionProcess;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.ViewModels;

public partial class ProductionProcessViewModel(ApiService apiService) : ViewModelBase
{
    #region Data Member

    [ObservableProperty] ProductionProcessHeader _productionProcessHeader = new();
    [ObservableProperty] ObservableCollection<GetProductionOrder> _getProductionOrder = new();
    [ObservableProperty] Boolean _isView;
    [ObservableProperty] PostResponse _postResponses = new();
    [ObservableProperty] ProcessProductionLine _processProductionLine = new();

    [ObservableProperty] List<string> _processType =
    [
        "PR1",
        "PR2",
        "PR3",
        "AGM",
        "SLT",
        "AFD",
        "FDC",
        "MSP",
        "FCM",
        "SST",
        "HST",
        "VST",
        "RGL",
        "AGL",
        "SGL",
        "Q-BOX",
        "ASY",
        "PAC"
    ];
    [ObservableProperty] string _token = string.Empty;
    #endregion

    public override async Task Loaded()
    {
        GetProductionOrder = (await apiService.GetProductionOrders("GetForProductionProcess",Token)).Data ?? new();
        IsView = true;
    }

    [RelayCommand]
    async Task Submit()
    {
        PostResponses = await apiService.PostProductionProcess(ProductionProcessHeader,Token);
    }
}