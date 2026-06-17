using System.Text.Json;
using System.Windows;
using ForexTradingWorkspace.Models.Export;

namespace ForexTradingWorkspace.Services.Export;

public sealed class AiReviewExportService : IAiReviewExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public Task<string> ExportToClipboardAsync(AiReviewPackage package)
    {
        var json = JsonSerializer.Serialize(package, JsonOptions);
        Clipboard.SetText(json);
        return Task.FromResult(json);
    }

    public async Task<string> SaveToFileAsync(AiReviewPackage package)
    {
        var folder = Path.Combine(AppPaths.DataPath, "AiReviewExports");
        Directory.CreateDirectory(folder);
        var file = Path.Combine(folder, $"ai-review-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        var json = JsonSerializer.Serialize(package, JsonOptions);
        await File.WriteAllTextAsync(file, json);
        return file;
    }
}
