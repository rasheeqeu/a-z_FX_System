using ForexTradingWorkspace.Models.Risk;
using ForexTradingWorkspace.Models.Screenshots;
using ForexTradingWorkspace.Models.Trading;

namespace ForexTradingWorkspace.Models.Export;

public sealed class AiReviewPackage
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? CurrentLessonId { get; set; }
    public string? CurrentLessonTitle { get; set; }
    public TradePlan Plan { get; set; } = new();
    public RiskResult? Risk { get; set; }
    public List<RuleCheckResult> RuleState { get; set; } = [];
    public List<ScreenshotRecord> Screenshots { get; set; } = [];
    public string JournalNotes { get; set; } = "";
    public List<string> MistakeTags { get; set; } = [];
    public string UserQuestion { get; set; } = "";
}
