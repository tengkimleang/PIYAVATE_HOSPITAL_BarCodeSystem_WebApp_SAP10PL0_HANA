

using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.Shared.Views.Shared.Component;

public partial class PrintLayout
{
    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    [Parameter]
    public Dictionary<string, object> Content { get; set; } = default!;

    private ObservableCollection<GetLayout> GetListData => Content["getLayout"] as ObservableCollection<GetLayout> ?? default!;
    private string DocEntry=> Content["docEntry"] as string ?? default!;
    private async Task OnPrint(string code)
    {
        Dialog.Hide();
        await JsRuntime.InvokeVoidAsync("window.open", $"{ApiConstant.ApiUrl}/layoutEndpoint?docEntry={DocEntry}&layoutCode={code}", "_blank");
    }

}