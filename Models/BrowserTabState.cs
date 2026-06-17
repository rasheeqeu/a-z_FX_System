namespace ForexTradingWorkspace.Models;

public sealed class BrowserTabState
{
    public string Title { get; set; } = "New Tab";
    public string Url { get; set; } = "https://www.tradingview.com/chart/";
    public bool IsPinned { get; set; }
}
