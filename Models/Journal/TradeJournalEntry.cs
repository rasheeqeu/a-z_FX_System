using ForexTradingWorkspace.Models.Risk;
using ForexTradingWorkspace.Models.Screenshots;
using ForexTradingWorkspace.Models.Trading;

namespace ForexTradingWorkspace.Models.Journal;

public sealed class TradeJournalEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public TradePlan Plan { get; set; } = new();
    public RiskResult? Risk { get; set; }
    public List<ScreenshotRecord> Screenshots { get; set; } = [];
    public decimal ProfitLoss { get; set; }
    public bool FollowedPlan { get; set; }
    public List<string> MistakeTags { get; set; } = [];
    public string LessonLearned { get; set; } = "";
    public string ReviewNotes { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
