using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.Views.ReturnRequest.MobileAppScreen;

public partial class ReturnRequestDashboard
{
    protected override void OnInitialized()
    {
        ComponentAttribute.Title = "Return Request";
        ComponentAttribute.Path = "/home";
        ComponentAttribute.IsBackButton = true;
    }
    private void OnFloatButtonClick()
    {
        // Handle the float button click event
    }
    void UpdateGridSize(GridItemSize size)
    {
        
    }

    private void OnClickList()
    {
        NavigationManager.NavigateTo("/ReturnRequest/Mobile/ListReturnRequest");
    }

    private void OnClickAddDeliveryOrderMobile()
    {
        NavigationManager.NavigateTo("/ReturnRequest/Mobile/Add/");
    }
    private void OnClickListSearchSalesOrder()
    {
        NavigationManager.NavigateTo("/ReturnRequest/Mobile/ListDeliveryOrder");
    }
}