namespace ForexTradingWorkspace.Models;

public class CalendarScreenState : ScreenState
{
    public string SelectedTab { get; set; } = "A";
    public string CurrentView { get; set; } = "monthly";

    public override bool IsValid => !string.IsNullOrWhiteSpace(SelectedTab);
}
