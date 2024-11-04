
using Microsoft.FluentUI.AspNetCore.Components;

namespace Piyavate_Hospital.Shared.Pages.ReturnRequest;

public partial class ReturnRequest
{
    private bool _isXs;
    // bool _visible=false;
    private bool _init;
    // protected void OnCloseOverlay() => _visible = true;
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