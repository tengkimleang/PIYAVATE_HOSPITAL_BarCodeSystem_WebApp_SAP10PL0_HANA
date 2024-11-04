
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.Views.Shared.Component;

public partial class MobileButtonPrint
{
    [Parameter] public ObservableCollection<GetLayout> GetLayouts { get; set; } = new();
    [Parameter] public string DocEntry { get; set; } = string.Empty;
    private async Task HandleOnMenuChanged(MenuChangeEventArgs args)
    {
        await JsRuntime.InvokeVoidAsync("window.open", $"{ApiConstant.ApiUrl}/layoutEndpoint?docEntry={DocEntry}&layoutCode={args.Id}");
    }
}