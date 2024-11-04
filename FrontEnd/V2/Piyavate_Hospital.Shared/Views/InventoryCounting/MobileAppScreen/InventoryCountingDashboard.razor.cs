using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.Views.InventoryCounting.MobileAppScreen;

public partial class InventoryCountingDashboard 
{
    protected override void OnInitialized()
    {
        ComponentAttribute.Title = "Inventory Transfer";
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
        NavigationManager.NavigateTo("/InventoryCounting/Mobile/ListInventoryCounting");
    }

    private void OnClickAddDeliveryOrderMobile()
    {
        NavigationManager.NavigateTo("/InventoryCounting/Mobile/Add/");
    }
}