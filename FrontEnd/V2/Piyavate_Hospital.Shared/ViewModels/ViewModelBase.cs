using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections;

namespace Piyavate_Hospital.Shared.ViewModels;

public abstract partial class ViewModelBase : ObservableObject, IViewModelBase
{
    public virtual async Task OnInitializedAsync()
    {
        await Loaded().ConfigureAwait(true);
    }

    protected virtual void NotifyStateChanged() => OnPropertyChanged((string?)null);
    protected virtual async Task<T> CheckingValueT<T>(T t, Func<Task<T>> func) where T : ICollection
    {
        if (t == null || t.Count == 0) return await func();
        return t;
    }
    [RelayCommand]
    public virtual async Task Loaded()
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
