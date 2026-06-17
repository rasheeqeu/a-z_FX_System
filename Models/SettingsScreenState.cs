namespace ForexTradingWorkspace.Models;

public class SettingsScreenState : ScreenState
{
    public Dictionary<string, string>? FormValues { get; set; } = new();
    public List<string>? ChecklistItems { get; set; } = [];
    public Dictionary<string, string>? EditorText { get; set; } = new();

    public override bool IsValid => true;
}
