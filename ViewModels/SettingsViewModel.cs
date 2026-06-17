using CommunityToolkit.Mvvm.ComponentModel;

namespace ForexTradingWorkspace.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private string broker = "XM.AE";
    [ObservableProperty] private string executionMode = "Manual demo execution";
    [ObservableProperty] private decimal maxRiskPercent = 2m;
    [ObservableProperty] private decimal minimumRewardRisk = 1.5m;

    // "claude" or "chatgpt"
    [ObservableProperty] private string aiTarget = "claude";
}
