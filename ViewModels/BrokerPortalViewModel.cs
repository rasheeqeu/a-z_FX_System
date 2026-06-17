using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForexTradingWorkspace.Models;

namespace ForexTradingWorkspace.ViewModels;

public partial class BrokerPortalViewModel : ObservableObject
{
    [ObservableProperty] private string activeUrl = "https://www.xm.ae/";
    [ObservableProperty] private string status = "Manual XM.AE demo execution. This app does not place trades.";

    public ObservableCollection<Bookmark> Bookmarks { get; } =
    [
        new() { Name = "XM.AE", Url = "https://www.xm.ae/" },
        new() { Name = "TradingView", Url = "https://www.tradingview.com/chart/" },
        new() { Name = "Forex Factory", Url = "https://www.forexfactory.com/calendar" },
        new() { Name = "Investing.com Calendar", Url = "https://www.investing.com/economic-calendar/" }
    ];

    [RelayCommand]
    private void OpenBookmark(Bookmark? bookmark)
    {
        if (bookmark is null) return;
        ActiveUrl = NormalizeUrl(bookmark.Url);
        Status = $"Opened {bookmark.Name}";
    }

    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "https://www.xm.ae/";
        var value = url.Trim();
        return value.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? value : $"https://{value}";
    }
}
