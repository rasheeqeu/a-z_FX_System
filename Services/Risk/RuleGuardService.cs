using ForexTradingWorkspace.Models.Risk;
using ForexTradingWorkspace.Models.Trading;

namespace ForexTradingWorkspace.Services.Risk;

public sealed class RuleGuardService : IRuleGuardService
{
    public IReadOnlyList<RuleCheckResult> Check(TradePlan plan, RiskResult? risk, bool checklistComplete, bool beforeScreenshotCaptured)
    {
        var results = new List<RuleCheckResult>
        {
            CheckRequired("Stop loss", plan.StopLoss > 0 && plan.StopLoss != plan.Entry, "A logical stop loss is required."),
            CheckRequired("Take profit", plan.TakeProfit > 0 && plan.TakeProfit != plan.Entry, "A take profit is required for reward/risk review."),
            CheckRequired("Reason", !string.IsNullOrWhiteSpace(plan.Reason), "Write why this demo trade exists."),
            CheckRequired("News checked", plan.NewsChecked, "Check news risk before practicing execution."),
            CheckRequired("Checklist", checklistComplete, "Complete the pre-trade checklist."),
            CheckRequired("Before screenshot", beforeScreenshotCaptured, "Capture what you saw before manual execution.")
        };

        if (risk is null)
        {
            results.Add(new RuleCheckResult { RuleName = "Risk calculated", Severity = RuleSeverity.Blocked, Message = "Calculate risk before marking the trade ready." });
        }
        else
        {
            results.Add(new RuleCheckResult
            {
                RuleName = "Risk limit",
                Severity = plan.RiskPercent <= 2m ? RuleSeverity.OK : RuleSeverity.Blocked,
                Message = plan.RiskPercent <= 2m ? "Risk is within the default beginner limit." : "Risk is above the 2% beginner limit."
            });
            results.Add(new RuleCheckResult
            {
                RuleName = "Reward/risk",
                Severity = risk.RewardRiskRatio >= 1.5m ? RuleSeverity.OK : RuleSeverity.Warning,
                Message = risk.RewardRiskRatio >= 1.5m ? "Reward/risk is acceptable for practice." : "Reward/risk is weak; explain why you still want this practice."
            });
        }

        return results;
    }

    private static RuleCheckResult CheckRequired(string name, bool ok, string blockedMessage)
    {
        return new RuleCheckResult
        {
            RuleName = name,
            Severity = ok ? RuleSeverity.OK : RuleSeverity.Blocked,
            Message = ok ? $"{name} complete." : blockedMessage
        };
    }
}
