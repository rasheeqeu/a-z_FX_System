using ForexTradingWorkspace.Models.Risk;
using ForexTradingWorkspace.Models.Trading;

namespace ForexTradingWorkspace.Services.Risk;

public interface IRuleGuardService
{
    IReadOnlyList<RuleCheckResult> Check(TradePlan plan, RiskResult? risk, bool checklistComplete, bool beforeScreenshotCaptured);
}
