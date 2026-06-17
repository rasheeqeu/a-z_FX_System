using ForexTradingWorkspace.Models;

namespace ForexTradingWorkspace.Services;

public interface IJournalExportService
{
    Task ExportCsvAsync(IEnumerable<Trade> trades, string filePath);
    Task ExportExcelAsync(IEnumerable<Trade> trades, string filePath);
    Task BackupDatabaseAsync(string filePath);
    Task RestoreDatabaseAsync(string filePath);
}
