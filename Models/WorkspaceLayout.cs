namespace ForexTradingWorkspace.Models;

public sealed class WorkspaceLayout
{
    public string ActiveProfile { get; set; } = "Demo";
    public double SidebarWidth { get; set; } = 72;
    public Dictionary<string, string> Profiles { get; set; } = new()
    {
        ["Scalping"] = "Dashboard|Browser|Calculator|Checklist",
        ["Swing Trading"] = "Dashboard|Browser|Journal|Analytics",
        ["News Trading"] = "Dashboard|Calendar|Browser|Checklist",
        ["Prop Firm"] = "Dashboard|Journal|Analytics|Checklist"
    };
}
