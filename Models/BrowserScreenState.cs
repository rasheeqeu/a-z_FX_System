namespace ForexTradingWorkspace.Models;

public class BrowserScreenState : ScreenState
{
    public string CurrentUrl { get; set; } = "https://www.tradingview.com/chart/";
    public string ActiveBrowserUrl { get; set; } = "https://www.tradingview.com/chart/";
    public DateTime? LastVisited { get; set; }

    public override bool IsValid => !string.IsNullOrWhiteSpace(CurrentUrl) && !string.IsNullOrWhiteSpace(ActiveBrowserUrl);
}
