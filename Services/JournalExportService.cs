using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using ForexTradingWorkspace.Models;

namespace ForexTradingWorkspace.Services;

public sealed class JournalExportService : IJournalExportService
{
    public async Task ExportCsvAsync(IEnumerable<Trade> trades, string filePath)
    {
        var builder = new StringBuilder();
        builder.AppendLine("OpenedAt,Pair,Direction,Entry,StopLoss,TakeProfit,Risk,ProfitLoss,Notes,BeforeScreenshot,AfterScreenshot");
        foreach (var trade in trades)
        {
            builder.AppendLine(string.Join(",", [
                trade.OpenedAt.ToString("O"),
                Escape(trade.Pair),
                Escape(trade.Direction),
                trade.Entry.ToString(CultureInfo.InvariantCulture),
                trade.StopLoss.ToString(CultureInfo.InvariantCulture),
                trade.TakeProfit.ToString(CultureInfo.InvariantCulture),
                trade.Risk.ToString(CultureInfo.InvariantCulture),
                trade.ProfitLoss.ToString(CultureInfo.InvariantCulture),
                Escape(trade.Notes),
                Escape(trade.BeforeScreenshotPath),
                Escape(trade.AfterScreenshotPath)
            ]));
        }

        await File.WriteAllTextAsync(filePath, builder.ToString(), Encoding.UTF8);
    }

    public Task ExportExcelAsync(IEnumerable<Trade> trades, string filePath)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Journal");
        var headers = new[] { "Opened", "Pair", "Direction", "Entry", "SL", "TP", "Risk", "P/L", "Notes", "Before", "After" };
        for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];

        var row = 2;
        foreach (var trade in trades)
        {
            sheet.Cell(row, 1).Value = trade.OpenedAt;
            sheet.Cell(row, 2).Value = trade.Pair;
            sheet.Cell(row, 3).Value = trade.Direction;
            sheet.Cell(row, 4).Value = trade.Entry;
            sheet.Cell(row, 5).Value = trade.StopLoss;
            sheet.Cell(row, 6).Value = trade.TakeProfit;
            sheet.Cell(row, 7).Value = trade.Risk;
            sheet.Cell(row, 8).Value = trade.ProfitLoss;
            sheet.Cell(row, 9).Value = trade.Notes;
            sheet.Cell(row, 10).Value = trade.BeforeScreenshotPath;
            sheet.Cell(row, 11).Value = trade.AfterScreenshotPath;
            row++;
        }

        sheet.Columns().AdjustToContents();
        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    public Task BackupDatabaseAsync(string filePath)
    {
        File.Copy(AppPaths.DatabaseFile, filePath, overwrite: true);
        return Task.CompletedTask;
    }

    public Task RestoreDatabaseAsync(string filePath)
    {
        File.Copy(filePath, AppPaths.DatabaseFile, overwrite: true);
        return Task.CompletedTask;
    }

    private static string Escape(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
