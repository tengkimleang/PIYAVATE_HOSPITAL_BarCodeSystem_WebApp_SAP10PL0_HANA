
using System.ComponentModel;

namespace Piyavate_Hospital.Shared.ViewModels;

public interface IViewModelBase : INotifyPropertyChanged
{
    Task OnInitializedAsync();
    Task Loaded();
}
