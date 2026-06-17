using ForexTradingWorkspace.Models.Export;

namespace ForexTradingWorkspace.Services.Export;

public interface IAiReviewExportService
{
    Task<string> ExportToClipboardAsync(AiReviewPackage package);
    Task<string> SaveToFileAsync(AiReviewPackage package);
}
