using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Collections.ObjectModel;
using Piyavate_Hospital.Shared.Models.Gets;

namespace Piyavate_Hospital.Shared.Views.GoodReceptPo;

public partial class ListBatchOrSerial
{
    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    [Parameter]
    public Dictionary<string, object> Content { get; set; } = default!;

    private ObservableCollection<GetBatchOrSerial> GetListData => Content["getData"] as ObservableCollection<GetBatchOrSerial> ?? default!;
    
    private string? dataGrid = "width: 1240px;height:300px;";

    private void UpdateGridSize(GridItemSize size)
    {
        dataGrid = size == GridItemSize.Xs ? "width: 1200px;height:205px" : "width: 100%;height:405px";
    }
}
