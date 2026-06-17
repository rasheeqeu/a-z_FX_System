namespace ForexTradingWorkspace.Models.Screenshots;

public sealed class ScreenshotRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string? LessonId { get; set; }
    public string? TradePlanId { get; set; }
    public string? JournalEntryId { get; set; }
    public ScreenshotCaptureType CaptureType { get; set; }
    public string Broker { get; set; } = "XM.AE";
    public string Instrument { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;
}
