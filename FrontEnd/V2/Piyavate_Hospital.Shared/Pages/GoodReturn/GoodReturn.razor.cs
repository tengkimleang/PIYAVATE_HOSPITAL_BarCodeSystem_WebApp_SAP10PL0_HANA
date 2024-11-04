
using Microsoft.FluentUI.AspNetCore.Components;

namespace Piyavate_Hospital.Shared.Pages.GoodReturn;

public partial class GoodReturn
{
    private bool _isXs = false;
    bool _visible = false;
    private bool _init = false;
    private void UpdateGridSize(GridItemSize size)
    {
        _init=true;
        if (size == GridItemSize.Xs)
        {
            _isXs = true;
        }
        else
        {
            _isXs = false;
        }
    }
}