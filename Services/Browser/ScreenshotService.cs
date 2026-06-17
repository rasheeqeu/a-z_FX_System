using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace ForexTradingWorkspace.Services.Browser;

public sealed class ScreenshotService : IScreenshotService
{
    public async Task<string> CaptureAsync(WebView2 webView, string symbol)
    {
        if (webView.CoreWebView2 is null)
        {
            await webView.EnsureCoreWebView2Async();
        }

        var cleanSymbol = string.Join("_", symbol.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        var folder = Path.Combine(AppPaths.ScreenshotsPath, DateTime.Now.ToString("yyyy-MM-dd"), cleanSymbol);
        Directory.CreateDirectory(folder);
        var file = Path.Combine(folder, $"{DateTime.Now:HHmmss}.png");

        var coreWebView = webView.CoreWebView2 ?? throw new InvalidOperationException("WebView2 is not initialized.");
        await using var stream = File.Create(file);
        await coreWebView.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
        return file;
    }
}
