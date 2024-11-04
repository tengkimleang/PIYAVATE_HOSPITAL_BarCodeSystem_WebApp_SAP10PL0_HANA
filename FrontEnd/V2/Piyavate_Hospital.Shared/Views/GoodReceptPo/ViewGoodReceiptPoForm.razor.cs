using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Collections.ObjectModel;
using Piyavate_Hospital.Shared.Models.Gets;
using Piyavate_Hospital.Shared.Pages;

namespace Piyavate_Hospital.Shared.Views.GoodReceptPo;

public partial class ViewGoodReceiptPoForm
{
    [Parameter]
    public Func<Task> AddNew { get; set; } = default!;
    [Parameter]
    public Func<Task> Search { get; set; } = default!;
    [Parameter]
    public Func<Task> PrintLayout { get; set; } = default!;
    [Parameter]
    public Func<Task> OnViewBatchOrSerial { get; set; } = default!;
    [Parameter]
    public GoodReceiptPoHeaderDeatialByDocNum? GetGoodReceiptPoHeaderDetailByDocNum { get; set; }
    [Parameter]
    public ObservableCollection<GoodReceiptPoLineByDocNum> GoodReceiptPoLineByDocNums { get; set; } = new ObservableCollection<GoodReceiptPoLineByDocNum>();
    [Parameter]
    public string Title { get; set; } = string.Empty;
    string dataGrid = "width: 100%;height:405px";
    void UpdateGridSize(GridItemSize size)
    {
        if (size == GridItemSize.Xs)
        {
            dataGrid = "width: 700px;height:205px";
        }
        else
        {
            dataGrid = "width: 100%;height:405px";
        }
    }
}
