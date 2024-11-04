using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.Views.GoodReturn.MobileAppScreen;

public partial class GoodReturnDashboard
{
    protected override void OnInitialized()
    {
        ComponentAttribute.Title = "Goods Return";
        ComponentAttribute.Path = "/home";
        ComponentAttribute.IsBackButton = true;
    }
    private void OnFloatButtonClick()
    {
        // Handle the float button click event
    }
    void UpdateGridSize(GridItemSize size)
    {
        // if (size == GridItemSize.Xs)
        // {
        //     stringDisplay = "";
        //     dataGrid = "width: 1600px;height:205px";
        //     fromWord = "";
        //     saveWord = "S-";
        // }
        // else
        // {
        //     stringDisplay = "Delivery Order";
        //     fromWord = "From";
        //     saveWord = "Save";
        //     dataGrid = "width: 1600px;height:405px";
        // }
    }

    private void OnClickList()
    {
        NavigationManager.NavigateTo("/GoodReturn/Mobile/ListGoodReturn");
    }

    private void OnClickAddDeliveryOrderMobile()
    {
        NavigationManager.NavigateTo("/GoodReturn/Mobile/Add/");
    }
    private void OnClickListSearchSalesOrder()
    {
        NavigationManager.NavigateTo("/GoodReturn/Mobile/ListGoodReceiptPo");
    }
}