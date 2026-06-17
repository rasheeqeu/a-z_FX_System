namespace ForexTradingWorkspace.Models;

public sealed class AppSettings
{
    public string Theme { get; set; } = "BlackRed";
    public int AutoLockMinutes { get; set; } = 15;
    public bool RequirePassword { get; set; }
    public string PasswordHash { get; set; } = "";
    public string DefaultBrowserUrl { get; set; } = "https://www.tradingview.com/chart/";
    public List<string> Profiles { get; set; } =
    [
        "Demo",
        "Real"
    ];
    public List<Bookmark> Bookmarks { get; set; } =
    [
        new() { Name = "TradingView", Url = "https://www.tradingview.com/chart/" },
        new() { Name = "Broker Terminal", Url = "https://www.metatrader5.com/en/terminal/web" },
        new() { Name = "Forex Factory", Url = "https://www.forexfactory.com/calendar" },
        new() { Name = "Myfxbook", Url = "https://www.myfxbook.com" },
        new() { Name = "Investing.com", Url = "https://www.investing.com/currencies/" }
    ];
    public List<BrowserTabState> LastTabs { get; set; } =
    [
        new() { Title = "TradingView", Url = "https://www.tradingview.com/chart/" }
    ];
    public List<string> ChecklistItems { get; set; } =
    [
        "Setup matches trading plan",
        "Risk is within daily limit",
        "Stop loss and take profit are placed",
        "No high impact news conflicts",
        "Psychology state is calm and focused"
    ];
}
