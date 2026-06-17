using Microsoft.Web.WebView2.Wpf;

namespace ForexTradingWorkspace.Services.Browser;

public interface IScreenshotService
{
    Task<string> CaptureAsync(WebView2 webView, string symbol);
}
