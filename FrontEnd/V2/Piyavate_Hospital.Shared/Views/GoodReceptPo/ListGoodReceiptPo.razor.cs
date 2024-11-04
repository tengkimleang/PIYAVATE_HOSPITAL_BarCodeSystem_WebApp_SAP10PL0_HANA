using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Piyavate_Hospital.Shared.Models.Gets;

namespace Piyavate_Hospital.Shared.Views.GoodReceptPo;

public partial class ListGoodReceiptPo
{

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    [Parameter]
    public Dictionary<string, object> Content { get; set; } = default!;
    
    private IEnumerable<TotalItemCount> TotalItemCounts => Content["totalItemCount"] as IEnumerable<TotalItemCount> ?? new List<TotalItemCount>();
    private bool IsDelete => Content.ContainsKey("isDelete");
    private bool IsSelete => Content.ContainsKey("isSelete");
    
    private Func<int, Task<ObservableCollection<GetListData>>> GetListData => Content["getData"] as Func<int, Task<ObservableCollection<GetListData>>>?? default!;

    private Func<string,Task> OnSeleteAsync => Content["onSelete"] as Func<string,Task> ?? default!;
    
    private Func<string,Task> OnDeleteAsync => Content["onDelete"] as Func<string,Task> ?? default!;
    private Func<Dictionary<string, object>, Task<ObservableCollection<GetListData>>> OnSearch => Content["onSearch"] as Func<Dictionary<string,object>, Task<ObservableCollection<GetListData>>> ?? default!;

    private string? dataGrid = "width: 1240px;height:300px;";
    
    private IEnumerable<GetListData> _goodReceiptPoHeaders= new List<GetListData>();
    
    PaginationState pagination = new() { ItemsPerPage = 10};

    private DateTime? dateFrom;
    private DateTime? dateTo;
    private string searchDocNum="";

    protected override async Task OnInitializedAsync()
    {
        int totalCount = Convert.ToInt32(TotalItemCounts.FirstOrDefault()?.AllItem ?? "0");
        await pagination.SetTotalItemCountAsync(totalCount).ConfigureAwait(false);
        await pagination.SetCurrentPageIndexAsync(0).ConfigureAwait(false);
        _goodReceiptPoHeaders = await GetListData(0);
    }

    private async Task LoadData(int page)
    {
        _goodReceiptPoHeaders =await GetListData(page);
    }
    
    private async Task SelectAsync(string docNum)
    {
        await Dialog.CloseAsync();
        await OnSeleteAsync(docNum);
    }

    private async Task OnClickSearch()
    {
        var searchParams = new Dictionary<string, object>
        {
            {"dateFrom", (dateFrom ?? Convert.ToDateTime("1999-01-01")).ToString("yyyy-MM-dd")},
            {"dateTo", (dateTo?? DateTime.Now.Date).ToString("yyyy-MM-dd")},
            {"docNum", searchDocNum}
        };
        _goodReceiptPoHeaders = await OnSearch(searchParams);
    }

    private void UpdateGridSize(GridItemSize size)
    {
        dataGrid = size == GridItemSize.Xs ? "width: 1200px;height:205px" : "width: 100%;height:405px";
    }
}