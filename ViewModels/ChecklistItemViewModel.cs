using CommunityToolkit.Mvvm.ComponentModel;

namespace ForexTradingWorkspace.ViewModels;

public partial class ChecklistItemViewModel(string text) : ObservableObject
{
    [ObservableProperty] private string text = text;
    [ObservableProperty] private bool isChecked;
}
