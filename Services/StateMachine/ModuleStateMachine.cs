using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ForexTradingWorkspace.Services.StateMachine;

public enum ModuleState
{
    Browser,
    Calendar,
    Journal,
    Settings,
    Dashboard,
    Empty
}

public partial class ModuleStateMachine : ObservableObject
{
    [ObservableProperty]
    private ModuleState currentState = ModuleState.Empty;

    [ObservableProperty]
    private bool shouldOpenSideScreen = false;

    [RelayCommand]
    public void NavigateTo(string moduleString)
    {
        if (string.IsNullOrEmpty(moduleString))
        {
            CurrentState = ModuleState.Empty;
            ShouldOpenSideScreen = false;
            return;
        }

        if (!Enum.TryParse<ModuleState>(moduleString, true, out var module))
        {
            CurrentState = ModuleState.Empty;
            ShouldOpenSideScreen = false;
            return;
        }

        CurrentState = module;

        // Determine if side screen should automatically open for this module
        ShouldOpenSideScreen = module switch
        {
            ModuleState.Browser => true,
            ModuleState.Calendar => true,
            ModuleState.Journal => true,
            ModuleState.Settings => false,
            ModuleState.Dashboard => false,
            ModuleState.Empty => false,
            _ => false
        };
    }
}
