using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ForexTradingWorkspace.Services.StateMachine;

public enum LayoutState
{
    SinglePanel,
    SplitPanel,
    SplitPanelSwapped
}

public partial class LayoutStateMachine : ObservableObject
{
    [ObservableProperty]
    private LayoutState currentState = LayoutState.SinglePanel;

    [RelayCommand]
    public void ToggleSplit()
    {
        CurrentState = CurrentState == LayoutState.SinglePanel
            ? LayoutState.SplitPanel
            : LayoutState.SinglePanel;
    }

    [RelayCommand]
    public void SwapLayout()
    {
        CurrentState = CurrentState switch
        {
            LayoutState.SplitPanel => LayoutState.SplitPanelSwapped,
            LayoutState.SplitPanelSwapped => LayoutState.SplitPanel,
            _ => CurrentState
        };
    }

    /// <summary>
    /// Force a specific layout state (used by modules to coordinate split panel visibility)
    /// </summary>
    public void SetState(LayoutState state)
    {
        CurrentState = state;
    }
}
